// <copyright file="FirefoxTargetManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable CS0067 // Temporal, do not merge with this
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;
using PuppeteerSharp.Helpers.Json;

namespace PuppeteerSharp.Cdp
{
    internal class FirefoxTargetManager : ITargetManager
    {
        private readonly Connection connection;
        private readonly Func<TargetInfo, CDPSession, CDPSession, CdpTarget> targetFactoryFunc;
        private readonly Func<CdpTarget, bool> targetFilterFunc;
        private readonly ILogger<FirefoxTargetManager> logger;
        private readonly AsyncDictionaryHelper<string, CdpTarget> availableTargetsByTargetId = new("Target {0} not found");
        private readonly ConcurrentDictionary<string, CdpTarget> availableTargetsBySessionId = new();
        private readonly ConcurrentDictionary<string, TargetInfo> discoveredTargetsByTargetId = new();
        private readonly TaskCompletionSource<bool> initializeCompletionSource = new();
        private List<string> targetsIdsForInit = [];

        public FirefoxTargetManager(
            Connection connection,
            Func<TargetInfo, CDPSession, CDPSession, CdpTarget> targetFactoryFunc,
            Func<Target, bool> targetFilterFunc)
        {
            this.connection = connection;
            this.targetFilterFunc = targetFilterFunc;
            this.targetFactoryFunc = targetFactoryFunc;
            this.logger = this.connection.LoggerFactory.CreateLogger<FirefoxTargetManager>();
            this.connection.MessageReceived += this.OnMessageReceived;
            this.connection.SessionDetached += this.Connection_SessionDetached;
        }

        public event EventHandler<TargetChangedArgs> TargetAvailable;

        public event EventHandler<TargetChangedArgs> TargetGone;

        public event EventHandler<TargetChangedArgs> TargetChanged;

        public event EventHandler<TargetChangedArgs> TargetDiscovered;

        public async Task InitializeAsync()
        {
            await this.connection.SendAsync("Target.setDiscoverTargets", new TargetSetDiscoverTargetsRequest
            {
                Discover = true,
                Filter = new[]
                {
                    new TargetSetDiscoverTargetsRequest.DiscoverFilter(),
                },
            }).ConfigureAwait(false);

            this.targetsIdsForInit = [.. this.discoveredTargetsByTargetId.Keys];
            await this.initializeCompletionSource.Task.ConfigureAwait(false);
        }

        public AsyncDictionaryHelper<string, CdpTarget> GetAvailableTargets() => this.availableTargetsByTargetId;

        public IEnumerable<ITarget> GetChildTargets(ITarget target) => [];

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "Target.attachedToTarget":
                        this.OnAttachedToTarget(sender, e.MessageData.ToObject<TargetAttachedToTargetResponse>());
                        return;
                    case "Target.targetCreated":
                        this.OnTargetCreated(e.MessageData.ToObject<TargetCreatedResponse>());
                        return;

                    case "Target.targetDestroyed":
                        this.OnTargetDestroyed(e.MessageData.ToObject<TargetDestroyedResponse>());
                        return;
                }
            }
            catch (Exception ex)
            {
                var message = $"Browser failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                this.connection.Close(message);
            }
        }

        private void Connection_SessionDetached(object sender, SessionEventArgs e)
            => e.Session.MessageReceived -= this.OnMessageReceived;

        private void OnTargetCreated(TargetCreatedResponse e)
        {
            if (!this.discoveredTargetsByTargetId.TryAdd(e.TargetInfo.TargetId, e.TargetInfo))
            {
                return;
            }

            if (e.TargetInfo.Type == TargetType.Browser && e.TargetInfo.Attached)
            {
                var browserTarget = this.targetFactoryFunc(e.TargetInfo, null, null);
                browserTarget.Initialize();
                this.availableTargetsByTargetId.AddItem(e.TargetInfo.TargetId, browserTarget);
                this.FinishInitializationIfReady(e.TargetInfo.TargetId);
            }

            var target = this.targetFactoryFunc(e.TargetInfo, null, null);
            if (this.targetFilterFunc != null && !this.targetFilterFunc(target))
            {
                this.FinishInitializationIfReady(e.TargetInfo.TargetId);
                return;
            }

            target.Initialize();
            this.availableTargetsByTargetId.AddItem(e.TargetInfo.TargetId, target);
            this.TargetAvailable?.Invoke(
                this,
                new TargetChangedArgs
                {
                    Target = target,
                    TargetInfo = e.TargetInfo,
                });
            this.FinishInitializationIfReady(target.TargetId);
        }

        private void OnTargetDestroyed(TargetDestroyedResponse e)
        {
            this.discoveredTargetsByTargetId.TryRemove(e.TargetId, out var targetInfo);
            this.FinishInitializationIfReady(e.TargetId);

            if (this.availableTargetsByTargetId.TryRemove(e.TargetId, out var target))
            {
                this.TargetGone?.Invoke(this, new TargetChangedArgs { Target = target, TargetInfo = targetInfo });
            }
        }

        private void OnAttachedToTarget(object sender, TargetAttachedToTargetResponse e)
        {
            var parentSession = sender as CDPSession;
            var targetInfo = e.TargetInfo;
            var session = this.connection.GetSession(e.SessionId) ?? throw new PuppeteerException($"Session {e.SessionId} was not created.");
            this.availableTargetsByTargetId.TryGetValue(targetInfo.TargetId, out var target);
            session.MessageReceived += this.OnMessageReceived;
            session.Target = target;
            this.availableTargetsBySessionId.TryAdd(session.Id, target);

            parentSession?.OnSessionReady(session);
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
    }
}
#pragma warning restore CS0067
