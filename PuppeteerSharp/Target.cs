// <copyright file="Target.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using PuppeteerSharp.Cdp;
    using PuppeteerSharp.Helpers;

    /// <summary>
    /// Target.
    /// </summary>
    [DebuggerDisplay("Target {Type} - {Url}")]
    public abstract class Target : ITarget
    {
        private readonly ConcurrentSet<ITarget> childTargets = [];

        internal Target(
            TargetInfo targetInfo,
            CDPSession session,
            BrowserContext context,
            ITargetManager targetManager,
            Func<bool, Task<CDPSession>> sessionFactory)
        {
            this.Session = session;
            this.TargetInfo = targetInfo;
            this.SessionFactory = sessionFactory;
            this.BrowserContext = context;
            this.TargetManager = targetManager;

            if (session != null)
            {
                session.Target = this;
            }
        }

        /// <inheritdoc/>
        public string Url => this.TargetInfo.Url;

        /// <inheritdoc/>
        public virtual TargetType Type => this.TargetInfo.Type;

        /// <inheritdoc/>
        public string TargetId => this.TargetInfo.TargetId;

        /// <inheritdoc/>
        public abstract ITarget Opener { get; }

        /// <inheritdoc/>
        IBrowser ITarget.Browser => this.Browser;

        /// <inheritdoc/>
        IBrowserContext ITarget.BrowserContext => this.BrowserContext;

        /// <inheritdoc/>
        IEnumerable<ITarget> ITarget.ChildTargets => this.childTargets;

        internal BrowserContext BrowserContext { get; }

        internal Browser Browser => this.BrowserContext.Browser;

        internal Task<InitializationStatus> InitializedTask => this.InitializedTaskWrapper.Task;

        internal TaskCompletionSource<InitializationStatus> InitializedTaskWrapper { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal Task CloseTask => this.CloseTaskWrapper.Task;

        internal TaskCompletionSource<bool> CloseTaskWrapper { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal Func<bool, Task<CDPSession>> SessionFactory { get; private set; }

        internal ITargetManager TargetManager { get; }

        internal bool IsInitialized { get; set; }

        internal CDPSession Session { get; }

        internal TargetInfo TargetInfo { get; set; }

        /// <inheritdoc/>
        public virtual Task<IPage> PageAsync() => Task.FromResult<IPage>(null);

        /// <inheritdoc/>
        public virtual Task<WebWorker> WorkerAsync() => Task.FromResult<WebWorker>(null);

        /// <inheritdoc/>
        public abstract Task<IPage> AsPageAsync();

        /// <inheritdoc/>
        public abstract Task<ICDPSession> CreateCDPSessionAsync();

        internal void AddChildTarget(CdpTarget target) => this.childTargets.Add(target);

        internal void RemoveChildTarget(CdpTarget target) => this.childTargets.Remove(target);
    }
}
