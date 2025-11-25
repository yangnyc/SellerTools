// <copyright file="NetworkManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    internal class NetworkManager
    {
        private readonly bool acceptInsecureCerts;
        private readonly NetworkEventManager networkEventManager = new();
        private readonly ILogger logger;
        private readonly ConcurrentSet<string> attemptedAuthentications = [];
        private readonly ConcurrentDictionary<ICDPSession, DisposableActionsStack> clients = new();
        private readonly IFrameProvider frameManager;
        private readonly ILoggerFactory loggerFactory;

        private InternalNetworkConditions emulatedNetworkConditions;
        private Dictionary<string, string> extraHTTPHeaders;
        private Credentials credentials;
        private bool userRequestInterceptionEnabled;
        private bool protocolRequestInterceptionEnabled;
        private bool? userCacheDisabled;
        private string userAgent;
        private UserAgentMetadata userAgentMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkManager"/> class.
        /// </summary>
        /// <param name="acceptInsecureCerts">If set to <c>true</c> ignore http errors.</param>
        /// <param name="frameManager">Frame manager.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        internal NetworkManager(bool acceptInsecureCerts, IFrameProvider frameManager, ILoggerFactory loggerFactory)
        {
            this.frameManager = frameManager;
            this.acceptInsecureCerts = acceptInsecureCerts;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<NetworkManager>();
        }

        internal event EventHandler<ResponseCreatedEventArgs> Response;

        internal event EventHandler<RequestEventArgs> Request;

        internal event EventHandler<RequestEventArgs> RequestFinished;

        internal event EventHandler<RequestEventArgs> RequestFailed;

        internal event EventHandler<RequestEventArgs> RequestServedFromCache;

        internal Dictionary<string, string> ExtraHTTPHeaders => this.extraHTTPHeaders?.Clone();

        internal int NumRequestsInProgress => this.networkEventManager.NumRequestsInProgress;

        internal Task AddClientAsync(ICDPSession client)
        {
            if (this.clients.ContainsKey(client))
            {
                return Task.CompletedTask;
            }

            var subscriptions = new DisposableActionsStack();
            this.clients[client] = subscriptions;
            client.MessageReceived += this.Client_MessageReceived;
            subscriptions.Defer(() => client.MessageReceived -= this.Client_MessageReceived);

            return Task.WhenAll(
                this.acceptInsecureCerts ? client.SendAsync("Security.setIgnoreCertificateErrors", new SecuritySetIgnoreCertificateErrorsRequest { Ignore = true }) : Task.CompletedTask,
                client.SendAsync("Network.enable"),
                this.ApplyExtraHTTPHeadersAsync(client),
                this.ApplyNetworkConditionsAsync(client),
                this.ApplyProtocolCacheDisabledAsync(client),
                this.ApplyProtocolRequestInterceptionAsync(client),
                this.ApplyUserAgentAsync(client));
        }

        internal void RemoveClient(CDPSession client)
        {
            if (!this.clients.TryRemove(client, out var subscriptions))
            {
                return;
            }

            subscriptions.Dispose();
        }

        internal async Task AuthenticateAsync(Credentials credentials)
        {
            this.credentials = credentials;
            var enabled = this.userRequestInterceptionEnabled || this.credentials != null;

            if (enabled == this.protocolRequestInterceptionEnabled)
            {
                return;
            }

            this.protocolRequestInterceptionEnabled = enabled;
            await this.ApplyToAllClientsAsync(this.ApplyProtocolRequestInterceptionAsync).ConfigureAwait(false);
        }

        internal Task SetExtraHTTPHeadersAsync(Dictionary<string, string> extraHTTPHeaders)
        {
            this.extraHTTPHeaders = [];

            foreach (var item in extraHTTPHeaders)
            {
                this.extraHTTPHeaders[item.Key.ToLower(CultureInfo.CurrentCulture)] = item.Value;
            }

            return this.ApplyToAllClientsAsync(this.ApplyExtraHTTPHeadersAsync);
        }

        internal Task SetUserAgentAsync(string userAgent, UserAgentMetadata userAgentMetadata)
        {
            this.userAgent = userAgent;
            this.userAgentMetadata = userAgentMetadata;
            return this.ApplyToAllClientsAsync(this.ApplyUserAgentAsync);
        }

        internal Task SetCacheEnabledAsync(bool enabled)
        {
            this.userCacheDisabled = !enabled;
            return this.ApplyToAllClientsAsync(this.ApplyProtocolCacheDisabledAsync);
        }

        internal async Task SetRequestInterceptionAsync(bool value)
        {
            this.userRequestInterceptionEnabled = value;
            var enabled = this.userRequestInterceptionEnabled || this.credentials != null;

            if (enabled == this.protocolRequestInterceptionEnabled)
            {
                return;
            }

            this.protocolRequestInterceptionEnabled = enabled;
            await this.ApplyToAllClientsAsync(this.ApplyProtocolRequestInterceptionAsync).ConfigureAwait(false);
        }

        internal Task SetOfflineModeAsync(bool value)
        {
            this.emulatedNetworkConditions ??= new InternalNetworkConditions();
            this.emulatedNetworkConditions.Offline = value;
            return this.ApplyToAllClientsAsync(this.ApplyNetworkConditionsAsync);
        }

        internal Task EmulateNetworkConditionsAsync(NetworkConditions networkConditions)
        {
            this.emulatedNetworkConditions ??= new InternalNetworkConditions();
            this.emulatedNetworkConditions.Upload = networkConditions?.Upload ?? -1;
            this.emulatedNetworkConditions.Download = networkConditions?.Download ?? -1;
            this.emulatedNetworkConditions.Latency = networkConditions?.Latency ?? 0;
            return this.ApplyToAllClientsAsync(this.ApplyNetworkConditionsAsync);
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var client = sender as CDPSession;
                switch (e.MessageID)
                {
                    case "Fetch.requestPaused":
                        await this.OnRequestPausedAsync(client, e.MessageData.ToObject<FetchRequestPausedResponse>()).ConfigureAwait(false);
                        break;
                    case "Fetch.authRequired":
                        await this.OnAuthRequiredAsync(client, e.MessageData.ToObject<FetchAuthRequiredResponse>()).ConfigureAwait(false);
                        break;
                    case "Network.requestWillBeSent":
                        await this.OnRequestWillBeSentAsync(client, e.MessageData.ToObject<RequestWillBeSentResponse>()).ConfigureAwait(false);
                        break;
                    case "Network.requestServedFromCache":
                        this.OnRequestServedFromCache(e.MessageData.ToObject<RequestServedFromCacheResponse>());
                        break;
                    case "Network.responseReceived":
                        this.OnResponseReceived(client, e.MessageData.ToObject<ResponseReceivedResponse>());
                        break;
                    case "Network.loadingFinished":
                        this.OnLoadingFinished(e.MessageData.ToObject<LoadingFinishedEventResponse>());
                        break;
                    case "Network.loadingFailed":
                        this.OnLoadingFailed(e.MessageData.ToObject<LoadingFailedEventResponse>());
                        break;
                    case "Network.responseReceivedExtraInfo":
                        await this.OnResponseReceivedExtraInfoAsync(sender as CDPSession, e.MessageData.ToObject<ResponseReceivedExtraInfoResponse>()).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"NetworkManager failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                _ = this.ApplyToAllClientsAsync(client =>
                {
                    (client as CDPSession)?.Close(message);
                    return Task.CompletedTask;
                });
            }
        }

        private async Task OnResponseReceivedExtraInfoAsync(CDPSession client, ResponseReceivedExtraInfoResponse e)
        {
            var redirectInfo = this.networkEventManager.TakeQueuedRedirectInfo(e.RequestId);

            if (redirectInfo != null)
            {
                this.networkEventManager.ResponseExtraInfo(e.RequestId).Add(e);
                await this.OnRequestAsync(client, redirectInfo.Event, redirectInfo.FetchRequestId).ConfigureAwait(false);
                return;
            }

            // We may have skipped response and loading events because we didn't have
            // this ExtraInfo event yet. If so, emit those events now.
            var queuedEvents = this.networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                this.networkEventManager.ForgetQueuedEventGroup(e.RequestId);
                this.EmitResponseEvent(client, queuedEvents.ResponseReceivedEvent, e);

                if (queuedEvents.LoadingFinishedEvent != null)
                {
                    this.EmitLoadingFinished(queuedEvents.LoadingFinishedEvent);
                }

                if (queuedEvents.LoadingFailedEvent != null)
                {
                    this.EmitLoadingFailed(queuedEvents.LoadingFailedEvent);
                }

                return;
            }

            // Wait until we get another event that can use this ExtraInfo event.
            this.networkEventManager.ResponseExtraInfo(e.RequestId).Add(e);
        }

        private void OnLoadingFailed(LoadingFailedEventResponse e)
        {
            var queuedEvents = this.networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                queuedEvents.LoadingFailedEvent = e;
            }
            else
            {
                this.EmitLoadingFailed(e);
            }
        }

        private void EmitLoadingFailed(LoadingFailedEventResponse e)
        {
            var request = this.networkEventManager.GetRequest(e.RequestId);
            if (request == null)
            {
                return;
            }

            request.FailureText = e.ErrorText;
            request.Response?.BodyLoadedTaskWrapper.TrySetResult(true);

            this.ForgetRequest(request, true);

            this.RequestFailed?.Invoke(this, new RequestEventArgs(request));
        }

        private void OnLoadingFinished(LoadingFinishedEventResponse e)
        {
            var queuedEvents = this.networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                queuedEvents.LoadingFinishedEvent = e;
            }
            else
            {
                this.EmitLoadingFinished(e);
            }
        }

        private void EmitLoadingFinished(LoadingFinishedEventResponse e)
        {
            var request = this.networkEventManager.GetRequest(e.RequestId);
            if (request == null)
            {
                return;
            }

            request.Response?.BodyLoadedTaskWrapper.TrySetResult(true);

            this.ForgetRequest(request, true);
            this.RequestFinished?.Invoke(this, new RequestEventArgs(request));
        }

        private void ForgetRequest(CdpHttpRequest request, bool events)
        {
            this.networkEventManager.ForgetRequest(request.Id);

            if (request.InterceptionId != null)
            {
                this.attemptedAuthentications.Remove(request.InterceptionId);
            }

            if (events)
            {
                this.networkEventManager.Forget(request.Id);
            }
        }

        private void OnResponseReceived(CDPSession client, ResponseReceivedResponse e)
        {
            var request = this.networkEventManager.GetRequest(e.RequestId);
            ResponseReceivedExtraInfoResponse extraInfo = null;

            if (request is { FromMemoryCache: false } && e.HasExtraInfo)
            {
                extraInfo = this.networkEventManager.ShiftResponseExtraInfo(e.RequestId);

                if (extraInfo == null)
                {
                    this.networkEventManager.QueuedEventGroup(e.RequestId, new()
                    {
                        ResponseReceivedEvent = e,
                    });
                    return;
                }
            }

            this.EmitResponseEvent(client, e, extraInfo);
        }

        private void EmitResponseEvent(CDPSession client, ResponseReceivedResponse e, ResponseReceivedExtraInfoResponse extraInfo)
        {
            var request = this.networkEventManager.GetRequest(e.RequestId);

            // FileUpload sends a response without a matching request.
            if (request == null)
            {
                return;
            }

            if (e.Response.FromDiskCache)
            {
                extraInfo = null;
            }

            var response = new CdpHttpResponse(
                client,
                request,
                e.Response,
                extraInfo);

            request.Response = response;

            this.Response?.Invoke(this, new ResponseCreatedEventArgs(response));
        }

        private async Task OnAuthRequiredAsync(CDPSession client, FetchAuthRequiredResponse e)
        {
            var response = "Default";
            if (this.attemptedAuthentications.Contains(e.RequestId))
            {
                response = "CancelAuth";
            }
            else if (this.credentials != null)
            {
                response = "ProvideCredentials";
                this.attemptedAuthentications.Add(e.RequestId);
            }

            var credentials = this.credentials ?? new Credentials();
            try
            {
                await client.SendAsync("Fetch.continueWithAuth", new ContinueWithAuthRequest
                {
                    RequestId = e.RequestId,
                    AuthChallengeResponse = new ContinueWithAuthRequestChallengeResponse
                    {
                        Response = response,
                        Username = credentials.Username,
                        Password = credentials.Password,
                    },
                }).ConfigureAwait(false);
            }
            catch (PuppeteerException ex)
            {
                this.logger.LogError(ex.ToString());
            }
        }

        private async Task OnRequestPausedAsync(CDPSession client, FetchRequestPausedResponse e)
        {
            if (!this.userRequestInterceptionEnabled && this.protocolRequestInterceptionEnabled)
            {
                try
                {
                    await client.SendAsync("Fetch.continueRequest", new FetchContinueRequestRequest
                    {
                        RequestId = e.RequestId,
                    }).ConfigureAwait(false);
                }
                catch (PuppeteerException ex)
                {
                    this.logger.LogError(ex.ToString());
                }
            }

            if (string.IsNullOrEmpty(e.NetworkId))
            {
                this.OnRequestWithoutNetworkInstrumentationAsync(client, e);
                return;
            }

            var requestWillBeSentEvent = this.networkEventManager.GetRequestWillBeSent(e.NetworkId);

            // redirect requests have the same `requestId`,
            if (
                requestWillBeSentEvent != null &&
                (requestWillBeSentEvent.Request.Url != e.Request.Url ||
                requestWillBeSentEvent.Request.Method != e.Request.Method))
            {
                this.networkEventManager.ForgetRequestWillBeSent(e.NetworkId);
                requestWillBeSentEvent = null;
            }

            if (requestWillBeSentEvent != null)
            {
                this.PatchRequestEventHeaders(requestWillBeSentEvent, e);
                await this.OnRequestAsync(client, requestWillBeSentEvent, e.RequestId).ConfigureAwait(false);
            }
            else
            {
                this.networkEventManager.StoreRequestPaused(e.NetworkId, e);
            }
        }

        private async void OnRequestWithoutNetworkInstrumentationAsync(CDPSession client, FetchRequestPausedResponse e)
        {
            // If an event has no networkId it should not have any network events. We
            // still want to dispatch it for the interception by the user.
            var frame = !string.IsNullOrEmpty(e.FrameId)
                ? await this.frameManager.GetFrameAsync(e.FrameId).ConfigureAwait(false)
                : null;

            var request = new CdpHttpRequest(
                    client,
                    frame,
                    e.RequestId,
                    this.userRequestInterceptionEnabled,
                    e,
                    [],
                    this.loggerFactory);

            this.Request?.Invoke(this, new RequestEventArgs(request));
            _ = request.FinalizeInterceptionsAsync();
        }

        private async Task OnRequestAsync(CDPSession client, RequestWillBeSentResponse e, string fetchRequestId)
        {
            CdpHttpRequest request;
            var redirectChain = new List<IRequest>();
            if (e.RedirectResponse != null)
            {
                ResponseReceivedExtraInfoResponse redirectResponseExtraInfo = null;
                if (e.RedirectHasExtraInfo)
                {
                    redirectResponseExtraInfo = this.networkEventManager.ShiftResponseExtraInfo(e.RequestId);
                    if (redirectResponseExtraInfo == null)
                    {
                        this.networkEventManager.QueueRedirectInfo(e.RequestId, new()
                        {
                            Event = e,
                            FetchRequestId = fetchRequestId,
                        });
                        return;
                    }
                }

                request = this.networkEventManager.GetRequest(e.RequestId);

                // If we connect late to the target, we could have missed the requestWillBeSent event.
                if (request != null)
                {
                    this.HandleRequestRedirect(client, request, e.RedirectResponse, redirectResponseExtraInfo);
                    redirectChain = request.RedirectChainList;
                }
            }

            var frame = !string.IsNullOrEmpty(e.FrameId) ? await this.frameManager.GetFrameAsync(e.FrameId).ConfigureAwait(false) : null;

            request = new CdpHttpRequest(
                client,
                frame,
                fetchRequestId,
                this.userRequestInterceptionEnabled,
                e,
                redirectChain,
                this.loggerFactory);

            this.networkEventManager.StoreRequest(e.RequestId, request);

            this.Request?.Invoke(this, new RequestEventArgs(request));

            try
            {
                await request.FinalizeInterceptionsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to FinalizeInterceptionsAsync");
            }
        }

        private void OnRequestServedFromCache(RequestServedFromCacheResponse response)
        {
            var request = this.networkEventManager.GetRequest(response.RequestId);

            if (request != null)
            {
                request.FromMemoryCache = true;
            }

            this.RequestServedFromCache?.Invoke(this, new RequestEventArgs(request));
        }

        private void HandleRequestRedirect(CDPSession client, CdpHttpRequest request, ResponsePayload responseMessage, ResponseReceivedExtraInfoResponse extraInfo)
        {
            var response = new CdpHttpResponse(
                client,
                request,
                responseMessage,
                extraInfo);

            request.Response = response;
            request.RedirectChainList.Add(request);
            response.BodyLoadedTaskWrapper.TrySetException(
                new PuppeteerException("Response body is unavailable for redirect responses"));

            this.ForgetRequest(request, false);

            this.Response?.Invoke(this, new ResponseCreatedEventArgs(response));
            this.RequestFinished?.Invoke(this, new RequestEventArgs(request));
        }

        private async Task OnRequestWillBeSentAsync(CDPSession client, RequestWillBeSentResponse e)
        {
            // Request interception doesn't happen for data URLs with Network Service.
            if (this.userRequestInterceptionEnabled && !e.Request.Url.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
            {
                this.networkEventManager.StoreRequestWillBeSent(e.RequestId, e);

                var requestPausedEvent = this.networkEventManager.GetRequestPaused(e.RequestId);
                if (requestPausedEvent != null)
                {
                    var fetchRequestId = requestPausedEvent.RequestId;
                    this.PatchRequestEventHeaders(e, requestPausedEvent);
                    await this.OnRequestAsync(client, e, fetchRequestId).ConfigureAwait(false);
                    this.networkEventManager.ForgetRequestPaused(e.RequestId);
                }

                return;
            }

            await this.OnRequestAsync(client, e, null).ConfigureAwait(false);
        }

        private void PatchRequestEventHeaders(RequestWillBeSentResponse requestWillBeSentEvent, FetchRequestPausedResponse requestPausedEvent)
        {
            foreach (var kv in requestPausedEvent.Request.Headers)
            {
                requestWillBeSentEvent.Request.Headers[kv.Key] = kv.Value;
            }
        }

        private async Task ApplyUserAgentAsync(ICDPSession client)
        {
            if (this.userAgent == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setUserAgentOverride",
                new NetworkSetUserAgentOverrideRequest
                {
                    UserAgent = this.userAgent,
                    UserAgentMetadata = this.userAgentMetadata,
                }).ConfigureAwait(false);
        }

        private async Task ApplyProtocolRequestInterceptionAsync(ICDPSession client)
        {
            this.userCacheDisabled ??= false;

            if (this.protocolRequestInterceptionEnabled)
            {
                await Task.WhenAll(
                    this.ApplyProtocolCacheDisabledAsync(client),
                    client.SendAsync(
                        "Fetch.enable",
                        new FetchEnableRequest
                        {
                            HandleAuthRequests = true,
                            Patterns = [new FetchEnableRequest.Pattern("*")],
                        })).ConfigureAwait(false);
            }
            else
            {
                await Task.WhenAll(
                    this.ApplyProtocolCacheDisabledAsync(client),
                    client.SendAsync("Fetch.disable")).ConfigureAwait(false);
            }
        }

        private async Task ApplyProtocolCacheDisabledAsync(ICDPSession client)
        {
            if (this.userCacheDisabled == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setCacheDisabled",
                new NetworkSetCacheDisabledRequest(this.userCacheDisabled.Value)).ConfigureAwait(false);
        }

        private async Task ApplyNetworkConditionsAsync(ICDPSession client)
        {
            if (this.emulatedNetworkConditions == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.emulateNetworkConditions",
                new NetworkEmulateNetworkConditionsRequest
                {
                    Offline = this.emulatedNetworkConditions.Offline,
                    Latency = this.emulatedNetworkConditions.Latency,
                    UploadThroughput = this.emulatedNetworkConditions.Upload,
                    DownloadThroughput = this.emulatedNetworkConditions.Download,
                }).ConfigureAwait(false);
        }

        private async Task ApplyExtraHTTPHeadersAsync(ICDPSession client)
        {
            if (this.extraHTTPHeaders == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setExtraHTTPHeaders",
                new NetworkSetExtraHTTPHeadersRequest(this.extraHTTPHeaders)).ConfigureAwait(false);
        }

        private Task ApplyToAllClientsAsync(Func<ICDPSession, Task> func)
            => Task.WhenAll(this.clients.Keys.Select(func));
    }
}
