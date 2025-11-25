// <copyright file="CdpBrowser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;

/// <inheritdoc />
public class CdpBrowser : Browser
{
    /// <summary>
    /// Time in milliseconds for process to exit gracefully.
    /// </summary>
    private const int CloseTimeout = 5000;

    private readonly ConcurrentDictionary<string, CdpBrowserContext> contexts;
    private readonly ILogger<Browser> logger;
    private Task closeTask;

    internal CdpBrowser(
        SupportedBrowser browser,
        Connection connection,
        string[] contextIds,
        bool acceptInsecureCerts,
        ViewPortOptions defaultViewport,
        LauncherBase launcher,
        Func<Target, bool> targetFilter = null,
        Func<Target, bool> isPageTargetFunc = null)
    {
        this.BrowserType = browser;
        this.AcceptInsecureCerts = acceptInsecureCerts;
        this.DefaultViewport = defaultViewport;
        this.Launcher = launcher;
        this.Connection = connection;
        var targetFilterCallback = targetFilter ?? (_ => true);
        this.logger = this.Connection.LoggerFactory.CreateLogger<Browser>();
        this.IsPageTargetFunc =
            isPageTargetFunc ??
            (target => target.Type is TargetType.Page or TargetType.BackgroundPage or TargetType.Webview);

        this.DefaultContext = new CdpBrowserContext(this.Connection, this, null);
        this.contexts = new ConcurrentDictionary<string, CdpBrowserContext>(
            contextIds.Select(contextId =>
                new KeyValuePair<string, CdpBrowserContext>(contextId, new(this.Connection, this, contextId))));

        if (browser == SupportedBrowser.Firefox)
        {
            this.TargetManager = new FirefoxTargetManager(
                    connection,
                    this.CreateTarget,
                    targetFilterCallback);
        }
        else
        {
            this.TargetManager = new ChromeTargetManager(
                connection,
                this.CreateTarget,
                targetFilterCallback,
                this,
                launcher?.Options?.Timeout ?? Puppeteer.DefaultTimeout);
        }
    }

    /// <inheritdoc />
    public override bool IsClosed
    {
        get
        {
            if (this.Launcher == null)
            {
                return this.Connection.IsClosed;
            }

            return this.closeTask is { IsCompleted: true };
        }
    }

    internal ITargetManager TargetManager { get; }

    /// <inheritdoc/>
    public override Task<IPage> NewPageAsync() => this.DefaultContext.NewPageAsync();

    /// <inheritdoc/>
    public override ITarget[] Targets()
        => this.TargetManager.GetAvailableTargets().Values.ToArray();

    /// <inheritdoc/>
    public override async Task<string> GetVersionAsync()
        => (await this.Connection.SendAsync<BrowserGetVersionResponse>("Browser.getVersion").ConfigureAwait(false)).Product;

    /// <inheritdoc/>
    public override async Task<string> GetUserAgentAsync()
        => (await this.Connection.SendAsync<BrowserGetVersionResponse>("Browser.getVersion").ConfigureAwait(false)).UserAgent;

    /// <inheritdoc/>
    public override void Disconnect()
    {
        this.Connection.Dispose();
        this.Detach();
    }

    /// <inheritdoc/>
    public override Task CloseAsync() => this.closeTask ??= this.CloseCoreAsync();

    /// <inheritdoc/>
    public override async Task<IBrowserContext> CreateBrowserContextAsync(BrowserContextOptions options = null)
    {
        var response = await this.Connection.SendAsync<CreateBrowserContextResponse>(
            "Target.createBrowserContext",
            new TargetCreateBrowserContextRequest
            {
                ProxyServer = options?.ProxyServer ?? string.Empty,
                ProxyBypassList = string.Join(",", options?.ProxyBypassList ?? Array.Empty<string>()),
            }).ConfigureAwait(false);
        var context = new CdpBrowserContext(this.Connection, this, response.BrowserContextId);
        this.contexts.TryAdd(response.BrowserContextId, (CdpBrowserContext)context);
        return context;
    }

    /// <inheritdoc/>
    public override IBrowserContext[] BrowserContexts() => [this.DefaultContext, .. this.contexts.Values];

    internal static async Task<CdpBrowser> CreateAsync(
        SupportedBrowser browserToCreate,
        Connection connection,
        string[] contextIds,
        bool acceptInsecureCerts,
        ViewPortOptions defaultViewPort,
        LauncherBase launcher,
        Func<Target, bool> targetFilter = null,
        Func<Target, bool> isPageTargetCallback = null,
        Action<IBrowser> initAction = null)
    {
        var browser = new CdpBrowser(
            browserToCreate,
            connection,
            contextIds,
            acceptInsecureCerts,
            defaultViewPort,
            launcher,
            targetFilter,
            isPageTargetCallback);

        try
        {
            initAction?.Invoke(browser);

            await browser.AttachAsync().ConfigureAwait(false);
            return browser;
        }
        catch
        {
            await browser.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    internal async Task<IPage> CreatePageInContextAsync(string contextId)
    {
        var createTargetRequest = new TargetCreateTargetRequest
        {
            Url = "about:blank",
        };

        if (contextId != null)
        {
            // We don't have this code in upstream.
            // Puppeteer sends a number if the contextId is a number, even if the typing says that it should be a string.
            // It seems that firefox ignores the contextId if it's not a number. Which is what Firefox sent back.
            createTargetRequest.BrowserContextId = int.TryParse(contextId, out var contextIdAsNumber)
                ? contextIdAsNumber
                : contextId;
        }

        var targetId = (await this.Connection.SendAsync<TargetCreateTargetResponse>("Target.createTarget", createTargetRequest)
            .ConfigureAwait(false)).TargetId;
        var target = await this.WaitForTargetAsync(t => t.TargetId == targetId).ConfigureAwait(false) as Target;
        await target!.InitializedTask.ConfigureAwait(false);
        return await target.PageAsync().ConfigureAwait(false);
    }

    internal async Task DisposeContextAsync(string contextId)
    {
        await this.Connection.SendAsync("Target.disposeBrowserContext", new TargetDisposeBrowserContextRequest
        {
            BrowserContextId = contextId,
        }).ConfigureAwait(false);
        this.contexts.TryRemove(contextId, out var _);
    }

    private Task AttachAsync()
    {
        this.Connection.Disconnected += this.Connection_Disconnected;
        this.TargetManager.TargetAvailable += this.OnAttachedToTargetAsync;
        this.TargetManager.TargetGone += this.OnDetachedFromTargetAsync;
        this.TargetManager.TargetChanged += this.OnTargetChanged;
        this.TargetManager.TargetDiscovered += this.TargetManager_TargetDiscovered;
        return this.TargetManager.InitializeAsync();
    }

    private void Detach()
    {
        this.Connection.Disconnected -= this.Connection_Disconnected;
        this.TargetManager.TargetAvailable -= this.OnAttachedToTargetAsync;
        this.TargetManager.TargetGone -= this.OnDetachedFromTargetAsync;
        this.TargetManager.TargetChanged -= this.OnTargetChanged;
        this.TargetManager.TargetDiscovered -= this.TargetManager_TargetDiscovered;
    }

    private CdpTarget CreateTarget(TargetInfo targetInfo, CDPSession session, CDPSession parentSession)
    {
        var browserContextId = targetInfo.BrowserContextId;

        if (!(browserContextId != null && this.contexts.TryGetValue(browserContextId, out var context)))
        {
            context = (CdpBrowserContext)this.DefaultContext;
        }

        Task<CDPSession> CreateSession(bool isAutoAttachEmulated) => this.Connection.CreateSessionAsync(targetInfo, isAutoAttachEmulated);

        var otherTarget = new CdpOtherTarget(
            targetInfo,
            session,
            context,
            this.TargetManager,
            CreateSession,
            this.ScreenshotTaskQueue);

        if (targetInfo.Url?.StartsWith("devtools://", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new CdpDevToolsTarget(
                targetInfo,
                session,
                context,
                this.TargetManager,
                CreateSession,
                this.AcceptInsecureCerts,
                this.DefaultViewport,
                this.ScreenshotTaskQueue);
        }

        if (this.IsPageTargetFunc(otherTarget))
        {
            return new CdpPageTarget(
                targetInfo,
                session,
                context,
                this.TargetManager,
                CreateSession,
                this.AcceptInsecureCerts,
                this.DefaultViewport,
                this.ScreenshotTaskQueue);
        }

        if (targetInfo.Type == TargetType.ServiceWorker || targetInfo.Type == TargetType.SharedWorker)
        {
            return new CdpWorkerTarget(
                targetInfo,
                session,
                context,
                this.TargetManager,
                CreateSession,
                this.ScreenshotTaskQueue);
        }

        return otherTarget;
    }

    private async Task CloseCoreAsync()
    {
        try
        {
            try
            {
                // Initiate graceful browser close operation but don't await it just yet,
                // because we want to ensure process shutdown first.
                var browserCloseTask = this.Connection.IsClosed
                    ? Task.CompletedTask
                    : this.Connection.SendAsync("Browser.close");

                if (this.Launcher != null)
                {
                    // Notify process that exit is expected, but should be enforced if it
                    // doesn't occur withing the close timeout.
                    var closeTimeout = TimeSpan.FromMilliseconds(CloseTimeout);
                    await this.Launcher.EnsureExitAsync(closeTimeout).ConfigureAwait(false);
                }

                // Now we can safely await the browser close operation without risking keeping chromium
                // process running for indeterminate period.
                await browserCloseTask.ConfigureAwait(false);
            }
            finally
            {
                this.Disconnect();
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);

            if (this.Launcher != null)
            {
                await this.Launcher.KillAsync().ConfigureAwait(false);
            }
        }

        this.OnClosed();
    }

    private async void Connection_Disconnected(object sender, EventArgs e)
    {
        try
        {
            await this.CloseAsync().ConfigureAwait(false);
            this.OnDisconnected();
        }
        catch (Exception ex)
        {
            var message = $"Browser failed to process Connection Close. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
            this.Connection.Close(message);
        }
    }

    private void TargetManager_TargetDiscovered(object sender, TargetChangedArgs e)
        => this.OnTargetDiscovered(e);

    private void OnTargetChanged(object sender, TargetChangedArgs e)
    {
        var args = new TargetChangedArgs { Target = e.Target };
        this.OnTargetChanged(args);
        ((CdpTarget)e.Target).BrowserContext.OnTargetChanged(this, args);
    }

    private async void OnDetachedFromTargetAsync(object sender, TargetChangedArgs e)
    {
        try
        {
            e.Target.InitializedTaskWrapper.TrySetResult(InitializationStatus.Aborted);
            e.Target.CloseTaskWrapper.TrySetResult(true);

            if ((await e.Target.InitializedTask.ConfigureAwait(false)) == InitializationStatus.Success)
            {
                var args = new TargetChangedArgs { Target = e.Target };
                this.OnTargetDestroyed(args);
                e.Target.BrowserContext.OnTargetDestroyed(this, args);
            }
        }
        catch (Exception ex)
        {
            var message = $"Browser failed to process Connection Close. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
            this.Connection.Close(message);
        }
    }

    private async void OnAttachedToTargetAsync(object sender, TargetChangedArgs e)
    {
        try
        {
            if (await e.Target.InitializedTask.ConfigureAwait(false) == InitializationStatus.Success)
            {
                var args = new TargetChangedArgs { Target = e.Target };
                this.OnTargetCreated(args);
                ((CdpTarget)e.Target).BrowserContext.OnTargetCreated(this, args);
            }
        }
        catch (Exception ex)
        {
            var message = $"Browser failed to process Target Available. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
        }
    }
}
