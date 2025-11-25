// <copyright file="CdpHttpRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Cdp.Messaging.Protocol.Network;

/// <inheritdoc/>
public class CdpHttpRequest : Request<CdpHttpResponse>
{
    private readonly CDPSession client;
    private readonly bool allowInterception;
    private readonly ILogger logger;
    private readonly List<Func<IRequest, Task>> interceptHandlers = [];
    private Payload continueRequestOverrides = new();
    private ResponseData responseForRequest;
    private RequestAbortErrorCode abortErrorReason;
    private InterceptResolutionState interceptResolutionState = new(InterceptResolutionAction.None);

    internal CdpHttpRequest(
        CDPSession client,
        IFrame frame,
        string interceptionId,
        bool allowInterception,
        RequestWillBeSentResponse data,
        List<IRequest> redirectChain,
        ILoggerFactory loggerFactory)
    {
        this.client = client;
        this.logger = loggerFactory.CreateLogger<CdpHttpRequest>();
        this.Id = data.RequestId;
        this.IsNavigationRequest = data.RequestId == data.LoaderId && data.Type == ResourceType.Document;
        this.InterceptionId = interceptionId;
        this.allowInterception = allowInterception;
        this.Url = data.Request.Url;
        this.ResourceType = data.Type ?? ResourceType.Other;
        this.Method = data.Request.Method;
        this.PostData = data.Request.PostData;
        this.HasPostData = data.Request.HasPostData ?? false;

        this.Frame = frame;
        this.RedirectChainList = redirectChain;
        this.Initiator = data.Initiator;

        this.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var keyValue in data.Request.Headers)
        {
            this.Headers[keyValue.Key] = keyValue.Value;
        }
    }

    /// <inheritdoc cref="Response"/>
    public override CdpHttpResponse Response { get; internal set; }

    internal override Payload ContinueRequestOverrides
    {
        get
        {
            if (!this.allowInterception)
            {
                throw new PuppeteerException("Request Interception is not enabled!");
            }

            return this.continueRequestOverrides;
        }
    }

    internal override ResponseData ResponseForRequest
    {
        get
        {
            if (!this.allowInterception)
            {
                throw new PuppeteerException("Request Interception is not enabled!");
            }

            return this.responseForRequest;
        }
    }

    internal override RequestAbortErrorCode AbortErrorReason
    {
        get
        {
            if (!this.allowInterception)
            {
                throw new PuppeteerException("Request Interception is not enabled!");
            }

            return this.abortErrorReason;
        }
    }

    private InterceptResolutionState InterceptResolutionState
    {
        get
        {
            if (!this.allowInterception)
            {
                return new InterceptResolutionState(InterceptResolutionAction.Disabled);
            }

            return this.IsInterceptResolutionHandled
                ? new InterceptResolutionState(InterceptResolutionAction.AlreadyHandled)
                : this.interceptResolutionState;
        }
    }

    private bool IsInterceptResolutionHandled { get; set; }

    /// <inheritdoc/>
    public override async Task ContinueAsync(Payload overrides = null, int? priority = null)
    {
        // Request interception is not supported for data: urls.
        if (this.Url.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        if (!this.allowInterception)
        {
            throw new PuppeteerException("Request Interception is not enabled!");
        }

        if (this.IsInterceptResolutionHandled)
        {
            throw new PuppeteerException("Request is already handled!");
        }

        if (priority is null)
        {
            await this.ContinueInternalAsync(overrides).ConfigureAwait(false);
            return;
        }

        this.continueRequestOverrides = overrides;

        if (this.interceptResolutionState.Priority is null || priority > this.interceptResolutionState.Priority)
        {
            this.interceptResolutionState = new InterceptResolutionState(InterceptResolutionAction.Continue, priority);
            return;
        }

        if (priority == this.interceptResolutionState.Priority)
        {
            if (this.interceptResolutionState.Action == InterceptResolutionAction.Abort ||
                this.interceptResolutionState.Action == InterceptResolutionAction.Respond)
            {
                return;
            }

            this.interceptResolutionState.Action = InterceptResolutionAction.Continue;
        }
    }

    /// <inheritdoc/>
    public override async Task RespondAsync(ResponseData response, int? priority = null)
    {
        if (this.Url.StartsWith("data:", StringComparison.Ordinal))
        {
            return;
        }

        if (!this.allowInterception)
        {
            throw new PuppeteerException("Request Interception is not enabled!");
        }

        if (this.IsInterceptResolutionHandled)
        {
            throw new PuppeteerException("Request is already handled!");
        }

        if (priority is null)
        {
            Debug.Assert(response != null, nameof(response) + " != null");
            await this.RespondInternalAsync(response).ConfigureAwait(false);
        }

        this.responseForRequest = response;

        if (this.interceptResolutionState.Priority is null || priority > this.interceptResolutionState.Priority)
        {
            this.interceptResolutionState = new InterceptResolutionState(InterceptResolutionAction.Respond, priority);
            return;
        }

        if (priority == this.interceptResolutionState.Priority)
        {
            if (this.interceptResolutionState.Action == InterceptResolutionAction.Abort)
            {
                return;
            }

            this.interceptResolutionState.Action = InterceptResolutionAction.Respond;
        }
    }

    /// <inheritdoc/>
    public override async Task AbortAsync(RequestAbortErrorCode errorCode = RequestAbortErrorCode.Failed, int? priority = null)
    {
        // Request interception is not supported for data: urls.
        if (this.Url.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        if (!this.allowInterception)
        {
            throw new PuppeteerException("Request Interception is not enabled!");
        }

        if (this.IsInterceptResolutionHandled)
        {
            throw new PuppeteerException("Request is already handled!");
        }

        if (priority is null)
        {
            await this.AbortInternalAsync(errorCode).ConfigureAwait(false);
            return;
        }

        this.abortErrorReason = errorCode;

        if (this.interceptResolutionState.Priority is null || priority > this.interceptResolutionState.Priority)
        {
            this.interceptResolutionState = new InterceptResolutionState(InterceptResolutionAction.Abort, priority);
        }
    }

    /// <inheritdoc />
    public override async Task<string> FetchPostDataAsync()
    {
        try
        {
            var result = await this.client.SendAsync<GetRequestPostDataResponse>(
                "Network.getRequestPostData",
                new GetRequestPostDataRequest(this.Id)).ConfigureAwait(false);
            return result.PostData;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.ToString());
        }

        return null;
    }

    internal override async Task FinalizeInterceptionsAsync()
    {
        foreach (var handler in this.interceptHandlers)
        {
            await handler(this).ConfigureAwait(false);
        }

        switch (this.InterceptResolutionState.Action)
        {
            case InterceptResolutionAction.Abort:
                await this.AbortAsync(this.abortErrorReason).ConfigureAwait(false);
                return;
            case InterceptResolutionAction.Respond:
                if (this.responseForRequest is null)
                {
                    throw new PuppeteerException("Response is missing for the interception");
                }

                await this.RespondAsync(this.responseForRequest).ConfigureAwait(false);
                return;
            case InterceptResolutionAction.Continue:
                await this.ContinueInternalAsync(this.continueRequestOverrides).ConfigureAwait(false);
                break;
        }
    }

    internal override void EnqueueInterceptionAction(Func<IRequest, Task> pendingHandler)
        => this.interceptHandlers.Add(pendingHandler);

    private Header[] HeadersArray(Dictionary<string, string> headers)
        => headers?.Select(pair => new Header { Name = pair.Key, Value = pair.Value }).ToArray();

    private async Task ContinueInternalAsync(Payload overrides = null)
    {
        this.IsInterceptResolutionHandled = true;

        try
        {
            var requestData = new FetchContinueRequestRequest
            {
                RequestId = this.InterceptionId,
            };
            if (overrides?.Url != null)
            {
                requestData.Url = overrides.Url;
            }

            if (overrides?.Method != null)
            {
                requestData.Method = overrides.Method.ToString();
            }

            if (overrides?.PostData != null)
            {
                requestData.PostData = Convert.ToBase64String(Encoding.UTF8.GetBytes(overrides.PostData));
            }

            if (overrides?.Headers?.Count > 0)
            {
                requestData.Headers = this.HeadersArray(overrides.Headers);
            }

            await this.client.SendAsync("Fetch.continueRequest", requestData).ConfigureAwait(false);
        }
        catch (PuppeteerException ex)
        {
            this.IsInterceptResolutionHandled = false;

            // In certain cases, protocol will return error if the request was already canceled
            // or the page was closed. We should tolerate these errors
            this.logger.LogError(ex.ToString());
        }
    }

    private async Task AbortInternalAsync(RequestAbortErrorCode errorCode)
    {
        var errorReason = errorCode.ToString();
        this.IsInterceptResolutionHandled = true;

        try
        {
            await this.client.SendAsync("Fetch.failRequest", new FetchFailRequest
            {
                RequestId = this.InterceptionId,
                ErrorReason = errorReason,
            }).ConfigureAwait(false);
        }
        catch (PuppeteerException ex)
        {
            // In certain cases, protocol will return error if the request was already canceled
            // or the page was closed. We should tolerate these errors
            this.logger.LogError(ex.ToString());
            this.IsInterceptResolutionHandled = false;
        }
    }

    private async Task RespondInternalAsync(ResponseData response)
    {
        this.IsInterceptResolutionHandled = true;

        var responseHeaders = new List<Header>();

        if (response.Headers != null)
        {
            foreach (var keyValuePair in response.Headers)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                if (keyValuePair.Value is ICollection values)
                {
                    foreach (var val in values)
                    {
                        responseHeaders.Add(new Header { Name = keyValuePair.Key, Value = val.ToString() });
                    }
                }
                else
                {
                    responseHeaders.Add(new Header { Name = keyValuePair.Key, Value = keyValuePair.Value.ToString() });
                }
            }

            if (!response.Headers.ContainsKey("content-length") && response.BodyData != null)
            {
                responseHeaders.Add(new Header { Name = "content-length", Value = response.BodyData.Length.ToString(CultureInfo.CurrentCulture) });
            }
        }

        if (response.ContentType != null)
        {
            responseHeaders.Add(new Header { Name = "content-type", Value = response.ContentType });
        }

        if (string.IsNullOrEmpty(this.InterceptionId))
        {
            throw new PuppeteerException("HTTPRequest is missing _interceptionId needed for Fetch.fulfillRequest");
        }

        try
        {
            await this.client.SendAsync("Fetch.fulfillRequest", new FetchFulfillRequest
            {
                RequestId = this.InterceptionId,
                ResponseCode = response.Status != null ? (int)response.Status : 200,
                ResponseHeaders = [.. responseHeaders],
                Body = response.BodyData != null ? Convert.ToBase64String(response.BodyData) : null,
            }).ConfigureAwait(false);
        }
        catch (PuppeteerException ex)
        {
            // In certain cases, protocol will return error if the request was already canceled
            // or the page was closed. We should tolerate these errors
            this.logger.LogError(ex.ToString());
            this.IsInterceptResolutionHandled = false;
        }
    }
}
