// <copyright file="ChromeTargetManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    internal class ChromeTargetManager : ITargetManager
    {
        private readonly List<string> ignoredTargets = new();
        private readonly Connection connection;
        private readonly Func<TargetInfo, CDPSession, CDPSession, CdpTarget> targetFactoryFunc;
        private readonly Func<Target, bool> targetFilterFunc;
        private readonly ILogger<ChromeTargetManager> logger;
        private readonly AsyncDictionaryHelper<string, CdpTarget> attachedTargetsByTargetId = new("Target {0} not found");
        private readonly ConcurrentDictionary<string, CdpTarget> attachedTargetsBySessionId = new();
        private readonly ConcurrentDictionary<string, TargetInfo> discoveredTargetsByTargetId = new();
        private readonly ConcurrentSet<string> targetsIdsForInit = [];
        private readonly TaskCompletionSource<bool> initializeCompletionSource = new();
        private readonly Browser browser;

        // Needed for .NET only to prevent race conditions between StoreExistingTargetsForInit and OnAttachedToTarget
        private readonly int targetDiscoveryTimeout;
        private readonly TaskCompletionSource<bool> targetDiscoveryCompletionSource = new();

        public ChromeTargetManager(
            Connection connection,
            Func<TargetInfo, CDPSession, CDPSession, CdpTarget> targetFactoryFunc,
            Func<Target, bool> targetFilterFunc,
            Browser browser,
            int targetDiscoveryTimeout = 0)
        {
            this.connection = connection;
            this.targetFilterFunc = targetFilterFunc;
            this.targetFactoryFunc = targetFactoryFunc;
            this.logger = this.connection.LoggerFactory.CreateLogger<ChromeTargetManager>();
            this.connection.MessageReceived += this.OnMessageReceived;
            this.connection.SessionDetached += this.Connection_SessionDetached;
            this.targetDiscoveryTimeout = targetDiscoveryTimeout;
            this.browser = browser;
        }

        public event EventHandler<TargetChangedArgs> TargetAvailable;

        public event EventHandler<TargetChangedArgs> TargetGone;

        public event EventHandler<TargetChangedArgs> TargetChanged;

        public event EventHandler<TargetChangedArgs> TargetDiscovered;

        public AsyncDictionaryHelper<string, CdpTarget> GetAvailableTargets() => this.attachedTargetsByTargetId;

        public async Task InitializeAsync()
        {
            try
            {
                await this.connection.SendAsync("Target.setDiscoverTargets", new TargetSetDiscoverTargetsRequest
                {
                    Discover = true,
                    Filter =
                    [
                        new TargetSetDiscoverTargetsRequest.DiscoverFilter() { Type = "tab", Exclude = true, },
                        new TargetSetDiscoverTargetsRequest.DiscoverFilter()
                    ],
                }).ConfigureAwait(false);
            }
            finally
            {
                this.targetDiscoveryCompletionSource.SetResult(true);
            }

            this.StoreExistingTargetsForInit();

            await this.connection.SendAsync(
                "Target.setAutoAttach",
                new TargetSetAutoAttachRequest()
                {
                    WaitForDebuggerOnStart = true,
                    Flatten = true,
                    AutoAttach = true,
                }).ConfigureAwait(false);

            this.FinishInitializationIfReady();

            await this.initializeCompletionSource.Task.ConfigureAwait(false);
        }

        public IEnumerable<ITarget> GetChildTargets(ITarget target) => target.ChildTargets;

        private void StoreExistingTargetsForInit()
        {
            foreach (var kv in this.discoveredTargetsByTargetId)
            {
                var targetForFilter = new CdpTarget(
                    kv.Value,
                    null,
                    null,
                    this,
                    null,
                    this.browser.ScreenshotTaskQueue);

                // Only wait for pages and frames (except those from extensions)
                // to auto-attach.
                var isPageOrFrame = kv.Value.Type is TargetType.Page or TargetType.IFrame;
                var isExtension = kv.Value.Url.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase);

                if (isPageOrFrame && !isExtension && (this.targetFilterFunc == null || this.targetFilterFunc(targetForFilter)))
                {
                    this.targetsIdsForInit.Add(kv.Key);
                }
            }
        }

        private async Task EnsureTargetsIdsForInitAsync()
        {
            if (this.targetDiscoveryTimeout > 0)
            {
                await this.targetDiscoveryCompletionSource.Task.WithTimeout(this.targetDiscoveryTimeout).ConfigureAwait(false);
            }
            else
            {
                await this.targetDiscoveryCompletionSource.Task.ConfigureAwait(false);
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "Target.attachedToTarget":
                        _ = this.OnAttachedToTargetHandlingExceptionsAsync(sender, e.MessageID, e.MessageData.ToObject<TargetAttachedToTargetResponse>());
                        return;

                    case "Target.detachedFromTarget":
                        this.OnDetachedFromTarget(sender, e.MessageData.ToObject<TargetDetachedFromTargetResponse>());
                        return;

                    case "Target.targetCreated":
                        this.OnTargetCreated(e.MessageData.ToObject<TargetCreatedResponse>());
                        return;

                    case "Target.targetDestroyed":
                        _ = this.OnTargetDestroyedAsync(e.MessageID, e.MessageData.ToObject<TargetDestroyedResponse>());
                        return;

                    case "Target.targetInfoChanged":
                        this.OnTargetInfoChanged(e.MessageData.ToObject<TargetCreatedResponse>());
                        return;
                }
            }
            catch (Exception ex)
            {
                this.HandleExceptionOnMessageReceived(e.MessageID, ex);
            }
        }

        private void Connection_SessionDetached(object sender, SessionEventArgs e)
        {
            e.Session.MessageReceived -= this.OnMessageReceived;
        }

        private void OnTargetCreated(TargetCreatedResponse e)
        {
            this.discoveredTargetsByTargetId[e.TargetInfo.TargetId] = e.TargetInfo;

            this.TargetDiscovered?.Invoke(this, new TargetChangedArgs { TargetInfo = e.TargetInfo });

            if (e.TargetInfo.Type == TargetType.Browser && e.TargetInfo.Attached)
            {
                if (this.attachedTargetsByTargetId.ContainsKey(e.TargetInfo.TargetId))
                {
                    return;
                }

                var target = this.targetFactoryFunc(e.TargetInfo, null, null);
                target.Initialize();
                this.attachedTargetsByTargetId.AddItem(e.TargetInfo.TargetId, target);
            }
        }

        private async Task OnTargetDestroyedAsync(string messageId, TargetDestroyedResponse e)
        {
            try
            {
                this.discoveredTargetsByTargetId.TryRemove(e.TargetId, out var targetInfo);
                await this.EnsureTargetsIdsForInitAsync().ConfigureAwait(false);
                this.FinishInitializationIfReady(e.TargetId);

                if (targetInfo?.Type == TargetType.ServiceWorker && this.attachedTargetsByTargetId.TryRemove(e.TargetId, out var target))
                {
                    this.TargetGone?.Invoke(this, new TargetChangedArgs { Target = target, TargetInfo = targetInfo });
                }
            }
            catch (Exception ex)
            {
                this.HandleExceptionOnMessageReceived(messageId, ex);
            }
        }

        private void OnTargetInfoChanged(TargetCreatedResponse e)
        {
            this.discoveredTargetsByTargetId[e.TargetInfo.TargetId] = e.TargetInfo;

            if (this.ignoredTargets.Contains(e.TargetInfo.TargetId) ||
                !this.attachedTargetsByTargetId.TryGetValue(e.TargetInfo.TargetId, out var target) ||
                !e.TargetInfo.Attached)
            {
                return;
            }

            var previousURL = target.Url;
            var wasInitialized = target.IsInitialized;

            if (this.IsPageTargetBecomingPrimary(target, e.TargetInfo))
            {
                var session = target.Session;
                session.ParentSession?.OnSwapped(session);
            }

            target.TargetInfoChanged(e.TargetInfo);

            if (wasInitialized && previousURL != target.Url)
            {
                this.TargetChanged?.Invoke(this, new TargetChangedArgs
                {
                    Target = target,
                    TargetInfo = e.TargetInfo,
                });
            }
        }

        private bool IsPageTargetBecomingPrimary(Target target, TargetInfo newTargetInfo)
            => !string.IsNullOrEmpty(target.TargetInfo.Subtype) && string.IsNullOrEmpty(newTargetInfo.Subtype);

        private async Task OnAttachedToTargetAsync(object sender, TargetAttachedToTargetResponse e)
        {
            var parentConnection = sender as ICDPConnection;
            var parentSession = sender as CDPSession;

            var targetInfo = e.TargetInfo;
            var session = this.connection.GetSession(e.SessionId) ?? throw new PuppeteerException($"Session {e.SessionId} was not created.");

            if (!this.connection.IsAutoAttached(targetInfo.TargetId))
            {
                return;
            }

            if (targetInfo.Type == TargetType.ServiceWorker)
            {
                await this.EnsureTargetsIdsForInitAsync().ConfigureAwait(false);
                this.FinishInitializationIfReady(targetInfo.TargetId);
                await SilentDetach().ConfigureAwait(false);
                if (this.attachedTargetsByTargetId.ContainsKey(targetInfo.TargetId))
                {
                    return;
                }

                var workerTarget = this.targetFactoryFunc(targetInfo, null, null);
                workerTarget.Initialize();
                this.attachedTargetsByTargetId.AddItem(targetInfo.TargetId, workerTarget);
                this.TargetAvailable?.Invoke(this, new TargetChangedArgs { Target = workerTarget });
                return;
            }

            var isExistingTarget = this.attachedTargetsByTargetId.TryGetValue(targetInfo.TargetId, out var target);
            if (!isExistingTarget)
            {
                target = this.targetFactoryFunc(targetInfo, session, parentSession);
            }

            if (this.targetFilterFunc?.Invoke(target) == false)
            {
                this.ignoredTargets.Add(targetInfo.TargetId);
                await this.EnsureTargetsIdsForInitAsync().ConfigureAwait(false);
                this.FinishInitializationIfReady(targetInfo.TargetId);
                await SilentDetach().ConfigureAwait(false);
                return;
            }

            session.MessageReceived += this.OnMessageReceived;

            if (isExistingTarget)
            {
                session.Target = target;
                this.attachedTargetsBySessionId.TryAdd(session.Id, target);
            }
            else
            {
                target.Initialize();

                this.attachedTargetsByTargetId.AddItem(targetInfo.TargetId, target);
                this.attachedTargetsBySessionId.TryAdd(session.Id, target);
            }

            var parentTarget = parentSession?.Target;
            parentTarget?.AddChildTarget(target);
            (parentSession ?? parentConnection as CDPSession)?.OnSessionReady(session);

            await this.EnsureTargetsIdsForInitAsync().ConfigureAwait(false);
            this.targetsIdsForInit.Remove(target.TargetId);

            if (!isExistingTarget)
            {
                this.TargetAvailable?.Invoke(this, new TargetChangedArgs { Target = target });
            }

            this.FinishInitializationIfReady();

            try
            {
                await Task.WhenAll(
                    session.SendAsync("Target.setAutoAttach", new TargetSetAutoAttachRequest
                    {
                        WaitForDebuggerOnStart = true,
                        Flatten = true,
                        AutoAttach = true,
                    }),
                    session.SendAsync("Runtime.runIfWaitingForDebugger")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to call setAutoAttach and runIfWaitingForDebugger");
            }

            return;

            async Task SilentDetach()
            {
                try
                {
                    await session.SendAsync("Runtime.runIfWaitingForDebugger").ConfigureAwait(false);
                    await parentConnection!.SendAsync(
                        "Target.detachFromTarget",
                        new TargetDetachFromTargetRequest
                        {
                            SessionId = session.Id,
                        }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "silentDetach failed.");
                }
            }
        }

        private async Task OnAttachedToTargetHandlingExceptionsAsync(object sender, string messageId, TargetAttachedToTargetResponse e)
        {
            try
            {
                await this.OnAttachedToTargetAsync(sender, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.HandleExceptionOnMessageReceived(messageId, ex);
            }
        }

        private void HandleExceptionOnMessageReceived(string messageId, Exception ex)
        {
            var message = $"Browser failed to process {messageId}. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
            this.connection.Close(message);
        }

        private void FinishInitializationIfReady(string targetId = null)
        {
            if (targetId != null)
            {
                this.targetsIdsForInit.Remove(targetId);
            }

            if (this.targetsIdsForInit.Count == 0)
            {
                this.initializeCompletionSource.TrySetResult(true);
            }
        }

        private void OnDetachedFromTarget(object sender, TargetDetachedFromTargetResponse e)
        {
            if (!this.attachedTargetsBySessionId.TryRemove(e.SessionId, out var target))
            {
                return;
            }

            (sender as CdpCDPSession)?.Target.RemoveChildTarget(target);
            this.attachedTargetsByTargetId.TryRemove(target.TargetId, out _);
            this.TargetGone?.Invoke(this, new TargetChangedArgs { Target = target });
        }
    }
}
