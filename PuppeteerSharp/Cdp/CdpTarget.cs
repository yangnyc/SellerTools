// <copyright file="CdpTarget.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Threading.Tasks;
using PuppeteerSharp.Helpers;

/// <inheritdoc />
public class CdpTarget : Target
{
    internal CdpTarget(
        TargetInfo targetInfo,
        CdpCDPSession session,
        CdpBrowserContext context,
        ITargetManager targetManager,
        Func<bool, Task<CDPSession>> sessionFactory,
        TaskQueue screenshotTaskQueue)
        : base(targetInfo, session, context, targetManager, sessionFactory)
    {
        this.ScreenshotTaskQueue = screenshotTaskQueue;
        this.Initialize();
    }

    /// <inheritdoc/>
    public override ITarget Opener => this.TargetInfo.OpenerId != null ?
        this.CdpBrowser.TargetManager.GetAvailableTargets().GetValueOrDefault(this.TargetInfo.OpenerId) : null;

    internal CdpBrowser CdpBrowser => this.Browser as CdpBrowser;

    internal TaskQueue ScreenshotTaskQueue { get; }

    /// <inheritdoc/>
    public override async Task<IPage> AsPageAsync()
    {
        if (this.Session == null)
        {
            var session = (CdpCDPSession)await this.CreateCDPSessionAsync().ConfigureAwait(false);
            return await CdpPage.CreateAsync(session, this, false, null, this.ScreenshotTaskQueue).ConfigureAwait(false);
        }

        return await CdpPage.CreateAsync((CdpCDPSession)this.Session, this, false, null, this.ScreenshotTaskQueue).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<ICDPSession> CreateCDPSessionAsync()
    {
        var session = await this.SessionFactory(false).ConfigureAwait(false);
        session.Target = this;
        return session;
    }

    internal void TargetInfoChanged(TargetInfo targetInfo)
    {
        this.TargetInfo = targetInfo;
        this.CheckIfInitialized();
    }

    /// <summary>
    /// Initializes the target.
    /// </summary>
    internal virtual void Initialize()
    {
        this.IsInitialized = true;
        this.InitializedTaskWrapper.TrySetResult(InitializationStatus.Success);
    }

    /// <summary>
    /// Check is the target is not initialized.
    /// </summary>
    protected internal virtual void CheckIfInitialized()
    {
        this.IsInitialized = true;
        this.InitializedTaskWrapper.TrySetResult(InitializationStatus.Success);
    }
}
