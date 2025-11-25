// <copyright file="CdpPageTarget.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;

    /// <summary>
    /// Page target.
    /// </summary>
    public class CdpPageTarget : CdpTarget
    {
        private readonly bool acceptInsecureCerts;
        private readonly ViewPortOptions defaultViewport;
        private readonly TaskQueue screenshotTaskQueue;

        internal CdpPageTarget(
            TargetInfo targetInfo,
            CDPSession session,
            BrowserContext context,
            ITargetManager targetManager,
            Func<bool, Task<CDPSession>> sessionFactory,
            bool acceptInsecureCerts,
            ViewPortOptions defaultViewport,
            TaskQueue screenshotTaskQueue)
            : base(targetInfo, (CdpCDPSession)session, (CdpBrowserContext)context, targetManager, sessionFactory, screenshotTaskQueue)
        {
            this.acceptInsecureCerts = acceptInsecureCerts;
            this.defaultViewport = defaultViewport;
            this.screenshotTaskQueue = screenshotTaskQueue;
            this.PageTask = null;
        }

        internal Task<Page> PageTask { get; set; }

        /// <inheritdoc/>
        public override async Task<IPage> PageAsync()
        {
            if (this.PageTask == null)
            {
                var session = (CdpCDPSession)(this.Session ?? await this.SessionFactory(false).ConfigureAwait(false));

                this.PageTask = CdpPage.CreateAsync(
                    session,
                    this,
                    this.acceptInsecureCerts,
                    this.defaultViewport,
                    this.screenshotTaskQueue);
            }

            return await this.PageTask.ConfigureAwait(false);
        }

        internal override void Initialize()
        {
            _ = this.InitializedTaskWrapper.Task.ContinueWith(
                async initializedTask =>
                {
                    var success = initializedTask.Result;
                    if (success != InitializationStatus.Success)
                    {
                        return;
                    }

                    var opener = this.Opener as CdpPageTarget;

                    var openerPageTask = opener?.PageTask;
                    if (openerPageTask == null || this.Type != TargetType.Page)
                    {
                        return;
                    }

                    var openerPage = await openerPageTask.ConfigureAwait(false);
                    if (!openerPage.HasPopupEventListeners)
                    {
                        return;
                    }

                    var popupPage = await this.PageAsync().ConfigureAwait(false);
                    openerPage.OnPopup(popupPage);
                },
                TaskScheduler.Default);
            this.CheckIfInitialized();
        }

        /// <inheritdoc/>
        protected internal override void CheckIfInitialized()
        {
            if (this.IsInitialized)
            {
                return;
            }

            this.IsInitialized = !string.IsNullOrEmpty(this.TargetInfo.Url);
            if (this.IsInitialized)
            {
                this.InitializedTaskWrapper.TrySetResult(InitializationStatus.Success);
            }
        }
    }
}
