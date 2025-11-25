// <copyright file="LifecycleWatcher.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;

    internal sealed class LifecycleWatcher : IDisposable
    {
        private static readonly WaitUntilNavigation[] DefaultWaitUntil = [WaitUntilNavigation.Load];

        private readonly NetworkManager networkManager;
        private readonly CdpFrame frame;
        private readonly IEnumerable<string> expectedLifecycle;
        private readonly int timeout;
        private readonly string initialLoaderId;
        private readonly TaskCompletionSource<bool> newDocumentNavigationTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> sameDocumentNavigationTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> lifecycleTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> terminationTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenSource terminationCancellationToken = new();
        private IRequest navigationRequest;
        private bool hasSameDocumentNavigation;
        private bool swapped;

        public LifecycleWatcher(
            NetworkManager networkManager,
            CdpFrame frame,
            WaitUntilNavigation[] waitUntil,
            int timeout)
        {
            this.expectedLifecycle = (waitUntil ?? DefaultWaitUntil).Select(w =>
            {
                var protocolEvent = GetProtocolEvent(w);
                Contract.Assert(protocolEvent != null, $"Unknown value for options.waitUntil: {w}");
                return protocolEvent;
            });

            this.networkManager = networkManager;
            this.frame = frame;
            this.initialLoaderId = frame.LoaderId;
            this.timeout = timeout;
            this.hasSameDocumentNavigation = false;

            frame.FrameManager.LifecycleEvent += this.FrameManager_LifecycleEvent;
            frame.FrameNavigatedWithinDocument += this.NavigatedWithinDocument;
            frame.FrameNavigated += this.Navigated;
            frame.FrameSwapped += this.FrameSwapped;
            frame.FrameSwappedByActivation += this.FrameSwapped;
            frame.FrameDetached += this.OnFrameDetached;
            this.networkManager.Request += this.OnRequest;
            this.CheckLifecycleComplete();
        }

        public Task<bool> SameDocumentNavigationTask => this.sameDocumentNavigationTaskWrapper.Task;

        public Task<bool> NewDocumentNavigationTask => this.newDocumentNavigationTaskWrapper.Task;

        public CdpHttpResponse NavigationResponse => (CdpHttpResponse)this.navigationRequest?.Response;

        public Task TerminationTask => this.terminationTaskWrapper.Task.WithTimeout(this.timeout, cancellationToken: this.terminationCancellationToken.Token);

        public Task LifecycleTask => this.lifecycleTaskWrapper.Task;

        public void Dispose()
        {
            this.frame.FrameManager.LifecycleEvent -= this.FrameManager_LifecycleEvent;
            this.frame.FrameNavigatedWithinDocument -= this.NavigatedWithinDocument;
            this.frame.FrameNavigated -= this.Navigated;
            this.frame.FrameDetached -= this.OnFrameDetached;
            this.frame.FrameSwapped -= this.FrameSwapped;
            this.networkManager.Request -= this.OnRequest;
            this.terminationCancellationToken.Cancel();
            this.terminationCancellationToken.Dispose();
        }

        private static string GetProtocolEvent(WaitUntilNavigation waitUntil) => waitUntil switch
        {
            WaitUntilNavigation.Load => "load",
            WaitUntilNavigation.DOMContentLoaded => "DOMContentLoaded",
            WaitUntilNavigation.Networkidle0 => "networkIdle",
            WaitUntilNavigation.Networkidle2 => "networkAlmostIdle",
            _ => null,
        };

        private void Navigated(object sender, FrameNavigatedEventArgs e)
        {
            if (e.Type == NavigationType.BackForwardCacheRestore)
            {
                this.FrameSwapped(sender, EventArgs.Empty);
            }

            this.CheckLifecycleComplete();
        }

        private void FrameManager_LifecycleEvent(object sender, FrameEventArgs e) => this.CheckLifecycleComplete();

        private void FrameSwapped(object sender, EventArgs e)
        {
            this.swapped = true;
            this.CheckLifecycleComplete();
        }

        private void OnFrameDetached(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            if (this.frame == frame)
            {
                var message = "Navigating frame was detached";

                if (!string.IsNullOrEmpty(frame?.Client?.CloseReason))
                {
                    message += $": {frame.Client.CloseReason}";
                }

                this.Terminate(new PuppeteerException(message));
                return;
            }

            this.CheckLifecycleComplete();
        }

        private void CheckLifecycleComplete()
        {
            // We expect navigation to commit.
            if (!this.CheckLifecycle(this.frame, this.expectedLifecycle))
            {
                return;
            }

            this.lifecycleTaskWrapper.TrySetResult(true);

            if (this.hasSameDocumentNavigation)
            {
                this.sameDocumentNavigationTaskWrapper.TrySetResult(true);
            }

            if (this.swapped || this.frame.LoaderId != this.initialLoaderId)
            {
                this.newDocumentNavigationTaskWrapper.TrySetResult(true);
            }
        }

        private void Terminate(PuppeteerException ex) => this.terminationTaskWrapper.TrySetException(ex);

        private void OnRequest(object sender, RequestEventArgs e)
        {
            if (e.Request.Frame != this.frame || !e.Request.IsNavigationRequest)
            {
                return;
            }

            this.navigationRequest = e.Request;
        }

        private void NavigatedWithinDocument(object sender, EventArgs e)
        {
            this.hasSameDocumentNavigation = true;
            this.CheckLifecycleComplete();
        }

        private bool CheckLifecycle(Frame frame, IEnumerable<string> expectedLifecycle)
        {
            var enumerable = expectedLifecycle as string[] ?? expectedLifecycle.ToArray();
            foreach (var item in enumerable)
            {
                if (!frame.LifecycleEvents.Contains(item))
                {
                    return false;
                }
            }

            foreach (var childFrame in frame.ChildFrames)
            {
                var child = (Frame)childFrame;
                if (child.HasStartedLoading && !this.CheckLifecycle(child, enumerable))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
