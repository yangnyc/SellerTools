// <copyright file="CdpWorkerTarget.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;

    /// <summary>
    /// Worker target.
    /// </summary>
    public class CdpWorkerTarget : CdpTarget
    {
        private Task<WebWorker> workerTask;

        internal CdpWorkerTarget(
            TargetInfo targetInfo,
            CDPSession session,
            BrowserContext context,
            ITargetManager targetManager,
            Func<bool, Task<CDPSession>> sessionFactory,
            TaskQueue screenshotTaskQueue)
            : base(targetInfo, (CdpCDPSession)session, (CdpBrowserContext)context, targetManager, sessionFactory, screenshotTaskQueue)
        {
        }

        /// <inheritdoc/>
        public override Task<WebWorker> WorkerAsync()
        {
            this.workerTask ??= this.WorkerInternalAsync();
            return this.workerTask;
        }

        private async Task<WebWorker> WorkerInternalAsync()
        {
            var client = this.Session ?? await this.SessionFactory(false).ConfigureAwait(false);
            return new CdpWebWorker(
                client,
                this.TargetInfo.Url,
                this.TargetInfo.TargetId,
                this.TargetInfo.Type,
                (_, _, _) => Task.CompletedTask,
                _ => { });
        }
    }
}
