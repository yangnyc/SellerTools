// <copyright file="FrameManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    internal class FrameManager : IDisposable, IAsyncDisposable, IFrameProvider
    {
        private const int TimeForWaitingForSwap = 200;
        private const string UtilityWorldName = "__puppeteer_utility_world__";

        private readonly ConcurrentDictionary<string, ExecutionContext> contextIdToContext = new();
        private readonly ILogger logger;
        private readonly List<string> isolatedWorlds = [];
        private readonly List<string> frameNavigatedReceived = [];
        private readonly TaskQueue eventsQueue = new();
        private readonly ConcurrentDictionary<CDPSession, DeviceRequestPromptManager> deviceRequestPromptManagerMap = new();
        private TaskCompletionSource<bool> frameTreeHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal FrameManager(CDPSession client, Page page, bool acceptInsecureCerts, TimeoutSettings timeoutSettings)
        {
            this.Client = client;
            this.Page = page;
            this.logger = this.Client.Connection.LoggerFactory.CreateLogger<FrameManager>();
            this.NetworkManager = new NetworkManager(acceptInsecureCerts, this, client.Connection.LoggerFactory);
            this.TimeoutSettings = timeoutSettings;

            this.Client.MessageReceived += this.Client_MessageReceived;
            this.Client.Disconnected += (sender, e) => _ = this.OnClientDisconnectAsync();
        }

        internal event EventHandler<FrameEventArgs> FrameAttached;

        internal event EventHandler<FrameEventArgs> FrameDetached;

        internal event EventHandler<FrameEventArgs> FrameSwapped;

        internal event EventHandler<FrameNavigatedEventArgs> FrameNavigated;

        internal event EventHandler<FrameEventArgs> FrameNavigatedWithinDocument;

        internal event EventHandler<FrameEventArgs> LifecycleEvent;

        internal CDPSession Client { get; private set; }

        internal NetworkManager NetworkManager { get; }

        internal Page Page { get; }

        internal TimeoutSettings TimeoutSettings { get; }

        internal FrameTree FrameTree { get; } = new();

        internal Frame MainFrame => this.FrameTree.MainFrame;

        public void Dispose() => this.eventsQueue?.Dispose();

        public async ValueTask DisposeAsync()
        {
            if (this.eventsQueue != null)
            {
                await this.eventsQueue.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Task<CdpFrame> GetFrameAsync(string frameId) => this.FrameTree.TryGetFrameAsync(frameId);

        internal ExecutionContext ExecutionContextById(int contextId, CDPSession session = null)
        {
            session ??= this.Client;
            var key = $"{session.Id}:{contextId}";
            this.contextIdToContext.TryGetValue(key, out var context);

            if (context == null)
            {
                this.logger.LogError("INTERNAL ERROR: missing context with id = {ContextId}", contextId);
            }

            return context;
        }

        internal void OnAttachedToTarget(TargetChangedArgs e)
        {
            if (e.TargetInfo.Type != TargetType.IFrame)
            {
                return;
            }

            var frame = this.GetFrame(e.TargetInfo.TargetId);
            frame?.UpdateClient(e.Target.Session);

            e.Target.Session.MessageReceived += this.Client_MessageReceived;
            _ = this.InitializeAsync(e.Target.Session);
        }

        internal ExecutionContext GetExecutionContextById(int contextId, CDPSession session)
        {
            this.contextIdToContext.TryGetValue($"{session.Id}:{contextId}", out var context);
            return context;
        }

        internal DeviceRequestPromptManager GetDeviceRequestPromptManager(CDPSession client)
            => this.deviceRequestPromptManagerMap.GetOrAdd(client, client => new DeviceRequestPromptManager(client, this.TimeoutSettings));

        internal Frame[] GetFrames() => this.FrameTree.Frames;

        internal async Task InitializeAsync(CDPSession client)
        {
            try
            {
                this.frameTreeHandled.TrySetResult(true);
                this.frameTreeHandled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var networkInitTask = this.NetworkManager.AddClientAsync(client);
                var getFrameTreeTask = client.SendAsync<PageGetFrameTreeResponse>("Page.getFrameTree");
                var autoAttachTask = client != this.Client
                    ? client.SendAsync("Target.setAutoAttach", new TargetSetAutoAttachRequest
                    {
                        AutoAttach = true,
                        WaitForDebuggerOnStart = false,
                        Flatten = true,
                    })
                    : Task.CompletedTask;

                await Task.WhenAll(
                    client.SendAsync("Page.enable"),
                    getFrameTreeTask,
                    autoAttachTask).ConfigureAwait(false);

                this.frameTreeHandled.TrySetResult(true);
                await this.HandleFrameTreeAsync(client, getFrameTreeTask.Result.FrameTree).ConfigureAwait(false);

                await Task.WhenAll(
                    client.SendAsync("Page.setLifecycleEventsEnabled", new PageSetLifecycleEventsEnabledRequest { Enabled = true }),
                    client.SendAsync("Runtime.enable"),
                    networkInitTask).ConfigureAwait(false);

                await this.CreateIsolatedWorldAsync(client, UtilityWorldName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.frameTreeHandled.TrySetResult(true);

                // The target might have been closed before the initialization finished.
                if (
                    ex.Message.Contains("Target closed") ||
                    ex.Message.Contains("Session closed"))
                {
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// When the main frame is replaced by another main frame
        /// we maintain the main frame object identity while updating
        /// its frame tree and ID.
        /// </summary>
        /// <param name="client">New session.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SwapFrameTreeAsync(CDPSession client)
        {
            this.OnExecutionContextsCleared(this.Client);

            this.Client = client;

            var frame = this.FrameTree.MainFrame;
            if (frame != null)
            {
                this.frameNavigatedReceived.Add(this.Client.Target.TargetId);
                this.FrameTree.RemoveFrame(frame);
                frame.Id = this.Client.Target.TargetId;
                frame.MainWorld.ClearContext();
                frame.PuppeteerWorld.ClearContext();
                this.FrameTree.AddFrame(frame);
                frame.UpdateClient(client, true);
            }

            this.Client.MessageReceived += this.Client_MessageReceived;
            this.Client.Disconnected += (sender, e) => _ = this.OnClientDisconnectAsync();

            await this.InitializeAsync(client).ConfigureAwait(false);
            await this.NetworkManager.AddClientAsync(client).ConfigureAwait(false);

            frame?.OnFrameSwappedByActivation();
        }

        internal Task RegisterSpeculativeSessionAsync(CDPSession client)
            => this.NetworkManager.AddClientAsync(client);

        private CdpFrame GetFrame(string frameId) => this.FrameTree.GetById(frameId);

        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            _ = this.eventsQueue.Enqueue(async () =>
            {
                try
                {
                    await this.frameTreeHandled.Task.WithTimeout().ConfigureAwait(false);
                    switch (e.MessageID)
                    {
                        case "Page.frameAttached":
                            this.OnFrameAttached(sender as CDPSession, e.MessageData.ToObject<PageFrameAttachedResponse>());
                            break;

                        case "Page.frameNavigated":
                            var response = e.MessageData.ToObject<PageFrameNavigatedResponse>();
                            await this.OnFrameNavigatedAsync(response.Frame, response.Type).ConfigureAwait(false);
                            break;

                        case "Page.navigatedWithinDocument":
                            this.OnFrameNavigatedWithinDocument(e.MessageData.ToObject<NavigatedWithinDocumentResponse>());
                            break;

                        case "Page.frameDetached":
                            this.OnFrameDetached(e.MessageData.ToObject<PageFrameDetachedResponse>());
                            break;

                        case "Page.frameStartedLoading":
                            this.OnFrameStartedLoading(e.MessageData.ToObject<BasicFrameResponse>());
                            break;

                        case "Page.frameStoppedLoading":
                            this.OnFrameStoppedLoading(e.MessageData.ToObject<BasicFrameResponse>());
                            break;

                        case "Runtime.executionContextCreated":
                            await this.OnExecutionContextCreatedAsync(e.MessageData.ToObject<RuntimeExecutionContextCreatedResponse>().Context, sender as CDPSession).ConfigureAwait(false);
                            break;

                        case "Runtime.executionContextDestroyed":
                            this.OnExecutionContextDestroyed(e.MessageData.ToObject<RuntimeExecutionContextDestroyedResponse>().ExecutionContextId, sender as CDPSession);
                            break;
                        case "Runtime.executionContextsCleared":
                            this.OnExecutionContextsCleared(sender as CDPSession);
                            break;
                        case "Page.lifecycleEvent":
                            this.OnLifeCycleEvent(e.MessageData.ToObject<LifecycleEventResponse>());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Connection failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                    this.logger.LogError(ex, message);
                    this.Client.Close(message);
                }
            });
        }

        private void OnFrameStartedLoading(BasicFrameResponse e)
        {
            var frame = this.GetFrame(e.FrameId);
            frame?.OnLoadingStarted();
        }

        private void OnFrameStoppedLoading(BasicFrameResponse e)
        {
            var frame = this.GetFrame(e.FrameId);
            if (frame != null)
            {
                frame.OnLoadingStopped();
                this.LifecycleEvent?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        private void OnLifeCycleEvent(LifecycleEventResponse e)
        {
            var frame = this.GetFrame(e.FrameId);
            if (frame != null)
            {
                frame.OnLifecycleEvent(e.LoaderId, e.Name);
                this.LifecycleEvent?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        private void OnExecutionContextsCleared(CDPSession session)
        {
            foreach (var key in this.contextIdToContext.Keys)
            {
                var context = this.contextIdToContext[key];
                if (context.Client != session)
                {
                    continue;
                }

                context.World?.ClearContext();

                this.contextIdToContext.TryRemove(key, out var _);
            }
        }

        private void OnExecutionContextDestroyed(int contextId, CDPSession session)
        {
            var key = $"{session.Id}:{contextId}";
#pragma warning disable CA2000
            if (this.contextIdToContext.TryRemove(key, out var context))
#pragma warning restore CA2000
            {
                context.World?.ClearContext();
            }
        }

        private async Task OnExecutionContextCreatedAsync(ContextPayload contextPayload, ICDPSession session)
        {
            var frameId = contextPayload.AuxData?.FrameId;
            var frame = !string.IsNullOrEmpty(frameId) ? await this.FrameTree.GetFrameAsync(frameId).ConfigureAwait(false) : null;
            IsolatedWorld world = null;

            if (frame != null)
            {
                if (frame.Client != session)
                {
                    return;
                }

                if (contextPayload.AuxData?.IsDefault == true)
                {
                    world = frame.MainWorld;
                }
                else if (contextPayload.Name == UtilityWorldName && !frame.PuppeteerWorld.HasContext)
                {
                    // In case of multiple sessions to the same target, there's a race between
                    // connections so we might end up creating multiple isolated worlds.
                    // We can use either.
                    world = frame.PuppeteerWorld;
                }
            }

            // If there is no world, the context is not meant to be handled by us.
            if (world == null)
            {
                return;
            }

            var context = new ExecutionContext(frame.Client ?? this.Client, contextPayload, world);
            world.SetContext(context);

            var key = $"{session.Id}:{contextPayload.Id}";
            this.contextIdToContext[key] = context;
        }

        private void OnFrameDetached(PageFrameDetachedResponse e)
        {
            var frame = this.GetFrame(e.FrameId);
            if (frame == null)
            {
                return;
            }

            if (e.Reason == FrameDetachedReason.Remove)
            {
                this.RemoveFramesRecursively(frame);
            }
            else if (e.Reason == FrameDetachedReason.Swap)
            {
                this.FrameSwapped?.Invoke(frame, new FrameEventArgs(frame));
                frame.OnSwapped();
            }
        }

        private async Task OnFrameNavigatedAsync(FramePayload framePayload, NavigationType type)
        {
            // This is in the event handler upstream.
            // It's more consistent having this here.
            this.frameNavigatedReceived.Add(framePayload.Id);

            var isMainFrame = string.IsNullOrEmpty(framePayload.ParentId);
            var frame = isMainFrame ? this.MainFrame : await this.FrameTree.GetFrameAsync(framePayload.Id).ConfigureAwait(false);

            Contract.Assert(isMainFrame || frame != null, "We either navigate top level or have old version of the navigated frame");

            // Detach all child frames first.
            if (frame != null)
            {
                while (frame.ChildFrames.Count > 0)
                {
                    this.RemoveFramesRecursively(frame.ChildFrames.First() as Frame);
                }
            }

            // Update or create main frame.
            if (isMainFrame)
            {
                if (frame != null)
                {
                    this.FrameTree.RemoveFrame(frame);
                    frame.Id = framePayload.Id;
                }
                else
                {
                    // Initial main frame navigation.
                    frame = new CdpFrame(this, framePayload.Id, null, this.Client);
                }

                this.FrameTree.AddFrame((CdpFrame)frame);
            }

            // Update frame payload.
            frame.Navigated(framePayload);
            frame.OnFrameNavigated(new FrameNavigatedEventArgs(frame, type));
            this.FrameNavigated?.Invoke(this, new FrameNavigatedEventArgs(frame, type));
        }

        private void OnFrameNavigatedWithinDocument(NavigatedWithinDocumentResponse e)
        {
            var frame = this.GetFrame(e.FrameId);
            if (frame != null)
            {
                frame.NavigatedWithinDocument(e.Url);

                var eventArgs = new FrameEventArgs(frame);
                this.FrameNavigatedWithinDocument?.Invoke(this, eventArgs);
                frame.OnFrameNavigated(new FrameNavigatedEventArgs(frame, NavigationType.Navigation));
                this.FrameNavigated?.Invoke(this, new FrameNavigatedEventArgs(frame, NavigationType.Navigation));
            }
        }

        private void RemoveFramesRecursively(Frame frame)
        {
            while (frame.ChildFrames.Count != 0)
            {
                this.RemoveFramesRecursively(frame.ChildFrames.First() as Frame);
            }

            frame.Detach();
            this.FrameTree.RemoveFrame(frame);
            this.FrameDetached?.Invoke(this, new FrameEventArgs(frame));
        }

        private void OnFrameAttached(CDPSession session, PageFrameAttachedResponse frameAttached)
            => this.OnFrameAttached(session, frameAttached.FrameId, frameAttached.ParentFrameId);

        private void OnFrameAttached(CDPSession session, string frameId, string parentFrameId)
        {
            var frame = this.GetFrame(frameId);
            if (frame != null)
            {
                if (session != null && frame.Client != this.Client)
                {
                    frame.UpdateClient(session);
                }

                return;
            }

            frame = new CdpFrame(this, frameId, parentFrameId, session);
            this.FrameTree.AddFrame(frame);
            this.FrameAttached?.Invoke(this, new FrameEventArgs(frame));
        }

        private async Task HandleFrameTreeAsync(CDPSession session, PageGetFrameTree frameTree)
        {
            if (!string.IsNullOrEmpty(frameTree.Frame.ParentId))
            {
                this.OnFrameAttached(session, frameTree.Frame.Id, frameTree.Frame.ParentId);
            }

            if (!this.frameNavigatedReceived.Contains(frameTree.Frame.Id))
            {
                await this.OnFrameNavigatedAsync(frameTree.Frame, NavigationType.Navigation).ConfigureAwait(false);
            }
            else
            {
                this.frameNavigatedReceived.Remove(frameTree.Frame.Id);
            }

            if (frameTree.ChildFrames != null)
            {
                foreach (var child in frameTree.ChildFrames)
                {
                    await this.HandleFrameTreeAsync(session, child).ConfigureAwait(false);
                }
            }
        }

        private async Task CreateIsolatedWorldAsync(CDPSession session, string name)
        {
            var key = $"{session.Id}:{name}";
            if (this.isolatedWorlds.Contains(key))
            {
                return;
            }

            this.isolatedWorlds.Add(key);
            await session.SendAsync("Page.addScriptToEvaluateOnNewDocument", new PageAddScriptToEvaluateOnNewDocumentRequest
            {
                Source = $"//# sourceURL={ExecutionContext.EvaluationScriptUrl}",
                WorldName = name,
            }).ConfigureAwait(false);

            try
            {
                await Task.WhenAll(this.GetFrames()
                    .Where(frame => frame.Client == session)
                    .Select(frame => session.SendAsync("Page.createIsolatedWorld", new PageCreateIsolatedWorldRequest
                    {
                        FrameId = frame.Id,
                        GrantUniveralAccess = true,
                        WorldName = name,
                    }))).ConfigureAwait(false);
            }
            catch (PuppeteerException ex)
            {
                this.logger.LogError(ex.ToString());
            }
        }

        private async Task OnClientDisconnectAsync()
        {
            try
            {
                var mainFrame = this.FrameTree.MainFrame;
                if (mainFrame == null)
                {
                    return;
                }

                foreach (var child in mainFrame.ChildFrames)
                {
                    this.RemoveFramesRecursively(child as Frame);
                }

                var swappedTcs = new TaskCompletionSource<bool>();

                mainFrame.FrameSwappedByActivation += (_, _) => swappedTcs.TrySetResult(true);

                try
                {
                    await swappedTcs.Task.WithTimeout(TimeForWaitingForSwap).ConfigureAwait(false);
                }
                catch
                {
                    this.RemoveFramesRecursively(mainFrame);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error while disconnecting");
            }
        }
    }
}
