// <copyright file="CdpDevToolsTarget.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;

    /// <summary>
    /// DevTools target.
    /// </summary>
    public class CdpDevToolsTarget : CdpPageTarget
    {
        internal CdpDevToolsTarget(
            TargetInfo targetInfo,
            CDPSession session,
            BrowserContext context,
            ITargetManager targetManager,
            Func<bool, Task<CDPSession>> sessionFactory,
            bool acceptInsecureCerts,
            ViewPortOptions defaultViewport,
            TaskQueue screenshotTaskQueue)
            : base(targetInfo, session, context, targetManager, sessionFactory, acceptInsecureCerts, defaultViewport, screenshotTaskQueue)
        {
        }

        /// <inheritdoc/>
        public override TargetType Type => TargetType.Other;
    }
}
