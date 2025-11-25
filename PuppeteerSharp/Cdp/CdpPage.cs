// <copyright file="CdpPage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Cdp.Messaging.Protocol.Network;
using PuppeteerSharp.Helpers;
using PuppeteerSharp.Helpers.Json;
using PuppeteerSharp.Media;
using PuppeteerSharp.PageAccessibility;
using PuppeteerSharp.PageCoverage;
using StackTrace = PuppeteerSharp.Cdp.Messaging.StackTrace;
using Timer = System.Timers.Timer;

/// <inheritdoc />
public class CdpPage : Page
{
    private readonly ConcurrentDictionary<string, CdpWebWorker> workers = new();
    private readonly ITargetManager targetManager;
    private readonly EmulationManager emulationManager;
    private readonly ILogger logger;
    private readonly Task closedFinishedTask;
    private readonly ConcurrentDictionary<string, Binding> bindings = new();
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<FileChooser>> fileChooserInterceptors = new();
    private readonly ConcurrentDictionary<string, string> exposedFunctions = new();
    private TaskCompletionSource<bool> sessionClosedTcs;

    private CdpPage(
        CdpCDPSession client,
        CdpTarget target,
        TaskQueue screenshotTaskQueue,
        bool acceptInsecureCerts)
        : base(screenshotTaskQueue)
    {
        this.PrimaryTargetClient = client;
        this.TabTargetClient = (CdpCDPSession)client.ParentSession;
        this.TabTarget = (CdpTarget)this.TabTargetClient.Target;
        this.PrimaryTarget = target;
        this.targetManager = target.TargetManager;
        this.Keyboard = new CdpKeyboard(client);
        this.Mouse = new CdpMouse(client, this.Keyboard);
        this.Touchscreen = new CdpTouchscreen(client, this.Keyboard);
        this.Tracing = new Tracing(client);
        this.Coverage = new Coverage(client);

        this.emulationManager = new EmulationManager(client);
        this.logger = this.Client.Connection.LoggerFactory.CreateLogger<Page>();
        this.FrameManager = new FrameManager(client, this, acceptInsecureCerts, this.TimeoutSettings);
        this.Accessibility = new Accessibility(client);

        this.FrameManager.FrameAttached += (_, e) => this.OnFrameAttached(e);
        this.FrameManager.FrameDetached += (_, e) => this.OnFrameDetached(e);
        this.FrameManager.FrameNavigated += (_, e) => this.OnFrameNavigated(e);

        this.FrameManager.NetworkManager.Request += (_, e) => this.OnRequest(e.Request);
        this.FrameManager.NetworkManager.RequestFailed += (_, e) => this.OnRequestFailed(e);
        this.FrameManager.NetworkManager.Response += (_, e) => this.OnResponse(e);
        this.FrameManager.NetworkManager.RequestFinished += (_, e) => this.OnRequestFinished(e);
        this.FrameManager.NetworkManager.RequestServedFromCache += (_, e) => this.OnRequestServedFromCache(e);

        this.TabTargetClient.Swapped += (sender, args) => _ = this.OnActivationAsync(args.Session as CdpCDPSession);
        this.TabTargetClient.Ready += (sender, args) => _ = this.OnSecondaryTargetAsync(args.Session as CdpCDPSession);
        this.targetManager.TargetGone += this.OnDetachedFromTarget;

        this.closedFinishedTask = this.TabTarget.CloseTask.ContinueWith(
            _ =>
            {
                try
                {
                    this.TabTarget.TargetManager.TargetGone -= this.OnDetachedFromTarget;
                    this.OnClose();
                }
                finally
                {
                    this.IsClosed = true;
                }
            },
            TaskScheduler.Default);

        this.SetupPrimaryTargetListeners();
        this.AttachExistingTargets();
    }

    /// <inheritdoc cref="CDPSession"/>
    public override CDPSession Client => this.PrimaryTargetClient;

    /// <inheritdoc cref="CDPSession"/>
    public override Target Target => this.PrimaryTarget;

    /// <inheritdoc/>
    public override IFrame MainFrame => this.FrameManager.MainFrame;

    /// <inheritdoc/>
    public override IFrame[] Frames => this.FrameManager.GetFrames();

    /// <inheritdoc/>
    public override WebWorker[] Workers => this.workers.Values.ToArray();

    /// <inheritdoc/>
    public override IBrowserContext BrowserContext => this.PrimaryTarget.BrowserContext;

    /// <inheritdoc/>
    public override bool IsJavaScriptEnabled => this.emulationManager.JavascriptEnabled;

    /// <inheritdoc />
    protected override Browser Browser => this.PrimaryTarget.Browser;

    private CdpCDPSession PrimaryTargetClient { get; set; }

    private CdpTarget PrimaryTarget { get; set; }

    private CdpCDPSession TabTargetClient { get; }

    private CdpTarget TabTarget { get; }

    private Task SessionClosedTask
    {
        get
        {
            if (this.sessionClosedTcs == null)
            {
                this.sessionClosedTcs =
                    new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                this.Client.Disconnected += ClientDisconnected;

                void ClientDisconnected(object sender, EventArgs e)
                {
                    this.sessionClosedTcs.TrySetException(new TargetClosedException("Target closed", "Session closed"));
                    this.Client.Disconnected -= ClientDisconnected;
                }
            }

            return this.sessionClosedTcs.Task;
        }
    }

    private FrameManager FrameManager { get; set; }

    /// <inheritdoc/>
    public override Task SetGeolocationAsync(GeolocationOption options)
        => this.emulationManager.SetGeolocationAsync(options);

    /// <inheritdoc/>
    public override Task SetDragInterceptionAsync(bool enabled)
    {
        this.IsDragInterceptionEnabled = enabled;
        return this.PrimaryTargetClient.SendAsync(
            "Input.setInterceptDrags",
            new InputSetInterceptDragsRequest { Enabled = enabled });
    }

    /// <inheritdoc/>
    public override async Task<CookieParam[]> GetCookiesAsync(params string[] urls)
    {
        if (urls == null)
        {
            throw new ArgumentNullException(nameof(urls));
        }

        return (await this.PrimaryTargetClient.SendAsync<NetworkGetCookiesResponse>(
                "Network.getCookies",
                new NetworkGetCookiesRequest { Urls = urls.Length > 0 ? urls : [this.Url], })
            .ConfigureAwait(false)).Cookies;
    }

    /// <inheritdoc/>
    public override async Task SetCookieAsync(params CookieParam[] cookies)
    {
        if (cookies == null)
        {
            throw new ArgumentNullException(nameof(cookies));
        }

        foreach (var cookie in cookies)
        {
            if (string.IsNullOrEmpty(cookie.Url) && this.Url.StartsWith("http", StringComparison.Ordinal))
            {
                cookie.Url = this.Url;
            }

            if (cookie.Url == "about:blank")
            {
                throw new PuppeteerException($"Blank page can not have cookie \"{cookie.Name}\"");
            }
        }

        await this.DeleteCookieAsync(cookies).ConfigureAwait(false);

        if (cookies.Length > 0)
        {
            await this.PrimaryTargetClient
                .SendAsync("Network.setCookies", new NetworkSetCookiesRequest { Cookies = cookies, })
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override async Task RemoveExposedFunctionAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!this.exposedFunctions.TryRemove(name, out var exposedFun) || !this.bindings.TryRemove(name, out _))
        {
            throw new PuppeteerException(
                $"Failed to remove page binding with name {name}: window['{name}'] does not exists!");
        }

        await this.Client.SendAsync("Runtime.removeBinding", new RuntimeRemoveBindingRequest { Name = name, })
            .ConfigureAwait(false);

        await this.RemoveScriptToEvaluateOnNewDocumentAsync(exposedFun).ConfigureAwait(false);

        await Task.WhenAll(
            this.Frames.Select(frame =>
                {
                    // If a frame has not started loading, it might never start. Rely on
                    // addScriptToEvaluateOnNewDocument in that case.
                    if (frame != this.MainFrame && !((Frame)frame).HasStartedLoading)
                    {
                        return Task.CompletedTask;
                    }

                    return frame.EvaluateFunctionAsync("name => globalThis[name] = undefined", name);
                })
                .ToArray()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task DeleteCookieAsync(params CookieParam[] cookies)
    {
        if (cookies == null)
        {
            throw new ArgumentNullException(nameof(cookies));
        }

        var pageURL = this.Url;
        foreach (var cookie in cookies)
        {
            if (string.IsNullOrEmpty(cookie.Url) && pageURL.StartsWith("http", StringComparison.Ordinal))
            {
                cookie.Url = pageURL;
            }

            await this.PrimaryTargetClient.SendAsync("Network.deleteCookies", cookie).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override async Task<Dictionary<string, decimal>> MetricsAsync()
    {
        var response = await this.Client.SendAsync<PerformanceGetMetricsResponse>("Performance.getMetrics")
            .ConfigureAwait(false);
        return this.BuildMetricsObject(response.Metrics);
    }

    /// <inheritdoc/>
    public override async Task<NewDocumentScriptEvaluation> EvaluateFunctionOnNewDocumentAsync(
        string pageFunction,
        params object[] args)
    {
        var source = BindingUtils.EvaluationString(pageFunction, args);
        var documentIdentifier = await this.Client
            .SendAsync<PageAddScriptToEvaluateOnNewDocumentResponse>(
                "Page.addScriptToEvaluateOnNewDocument",
                new PageAddScriptToEvaluateOnNewDocumentRequest { Source = source, }).ConfigureAwait(false);

        return new NewDocumentScriptEvaluation(documentIdentifier.Identifier);
    }

    /// <inheritdoc/>
    public override Task RemoveScriptToEvaluateOnNewDocumentAsync(string identifier)
        => this.Client.SendAsync("Page.removeScriptToEvaluateOnNewDocument", new PageRemoveScriptToEvaluateOnNewDocumentRequest
        {
            Identifier = identifier,
        });

    /// <inheritdoc />
    public override Task SetBypassServiceWorkerAsync(bool bypass)
    {
        this.IsServiceWorkerBypassed = bypass;
        return this.Client.SendAsync("Network.setBypassServiceWorker", new SetBypassServiceWorkerRequest
        {
            Bypass = bypass,
        });
    }

    /// <inheritdoc/>
    public override async Task<NewDocumentScriptEvaluation> EvaluateExpressionOnNewDocumentAsync(string expression)
    {
        var documentIdentifier = await
            this.Client.SendAsync<PageAddScriptToEvaluateOnNewDocumentResponse>(
                "Page.addScriptToEvaluateOnNewDocument",
                new PageAddScriptToEvaluateOnNewDocumentRequest { Source = expression, }).ConfigureAwait(false);

        return new NewDocumentScriptEvaluation(documentIdentifier.Identifier);
    }

    /// <inheritdoc/>
    public override async Task<IJSHandle> QueryObjectsAsync(IJSHandle prototypeHandle)
    {
        if (prototypeHandle == null)
        {
            throw new ArgumentNullException(nameof(prototypeHandle));
        }

        if (prototypeHandle.Disposed)
        {
            throw new PuppeteerException("Prototype JSHandle is disposed!");
        }

        if (prototypeHandle.RemoteObject.ObjectId == null)
        {
            throw new PuppeteerException("Prototype JSHandle must not be referencing primitive value");
        }

        var response = await this.Client.SendAsync<RuntimeQueryObjectsResponse>(
                "Runtime.queryObjects",
                new RuntimeQueryObjectsRequest { PrototypeObjectId = prototypeHandle.RemoteObject.ObjectId, })
            .ConfigureAwait(false);

        var context = await this.FrameManager.MainFrame.MainWorld.GetExecutionContextAsync().ConfigureAwait(false);
        return context.CreateJSHandle(response.Objects);
    }

    /// <inheritdoc/>
    public override Task SetRequestInterceptionAsync(bool value)
        => this.FrameManager.NetworkManager.SetRequestInterceptionAsync(value);

    /// <inheritdoc/>
    public override Task SetOfflineModeAsync(bool value) => this.FrameManager.NetworkManager.SetOfflineModeAsync(value);

    /// <inheritdoc/>
    public override Task SetJavaScriptEnabledAsync(bool enabled)
        => this.emulationManager.SetJavaScriptEnabledAsync(enabled);

    /// <inheritdoc/>
    public override Task SetBypassCSPAsync(bool enabled) => this.PrimaryTargetClient.SendAsync(
        "Page.setBypassCSP",
        new PageSetBypassCSPRequest { Enabled = enabled, });

    /// <inheritdoc/>
    public override Task EmulateMediaTypeAsync(MediaType type)
        => this.emulationManager.EmulateMediaTypeAsync(type);

    /// <inheritdoc/>
    public override Task EmulateMediaFeaturesAsync(IEnumerable<MediaFeatureValue> features)
        => this.emulationManager.EmulateMediaFeaturesAsync(features);

    /// <inheritdoc/>
    public override async Task SetViewportAsync(ViewPortOptions viewport)
    {
        if (viewport == null)
        {
            throw new ArgumentNullException(nameof(viewport));
        }

        var needsReload = await this.emulationManager.EmulateViewportAsync(viewport).ConfigureAwait(false);
        this.Viewport = viewport;

        if (needsReload)
        {
            await this.ReloadAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override Task EmulateNetworkConditionsAsync(NetworkConditions networkConditions)
        => this.FrameManager.NetworkManager.EmulateNetworkConditionsAsync(networkConditions);

    /// <inheritdoc/>
    public override Task SetCacheEnabledAsync(bool enabled = true)
        => this.FrameManager.NetworkManager.SetCacheEnabledAsync(enabled);

    /// <inheritdoc/>
    public override Task SetUserAgentAsync(string userAgent, UserAgentMetadata userAgentData = null)
        => this.FrameManager.NetworkManager.SetUserAgentAsync(userAgent, userAgentData);

    /// <inheritdoc/>
    public override async Task<IResponse> ReloadAsync(NavigationOptions options)
    {
        Debug.Assert(options != null, nameof(options) + " != null");
        var navigationTask = this.WaitForNavigationAsync(options with { IgnoreSameDocumentNavigation = true });

        await Task.WhenAll(
                navigationTask,
                this.PrimaryTargetClient.SendAsync("Page.reload", new PageReloadRequest { FrameId = this.MainFrame.Id }))
            .ConfigureAwait(false);

        return navigationTask.Result;
    }

    /// <inheritdoc/>
    public override async Task WaitForNetworkIdleAsync(WaitForNetworkIdleOptions options = null)
    {
        var timeout = options?.Timeout ?? this.DefaultTimeout;
        var idleTime = options?.IdleTime ?? 500;

        var networkIdleTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var idleTimer = new Timer { Interval = idleTime, };

        idleTimer.Elapsed += (_, _) => { networkIdleTcs.TrySetResult(true); };

        var networkManager = this.FrameManager.NetworkManager;

        void Evaluate()
        {
            idleTimer.Stop();

            if (networkManager.NumRequestsInProgress == 0)
            {
                idleTimer.Start();
            }
        }

        void RequestEventListener(object sender, RequestEventArgs e) => Evaluate();
        void ResponseEventListener(object sender, ResponseCreatedEventArgs e) => Evaluate();

        void Cleanup()
        {
            idleTimer.Stop();
            idleTimer.Dispose();

            networkManager.Request -= RequestEventListener;
            networkManager.Response -= ResponseEventListener;
        }

        networkManager.Request += RequestEventListener;
        networkManager.Response += ResponseEventListener;

        Evaluate();

        await Task.WhenAny(networkIdleTcs.Task, this.SessionClosedTask).WithTimeout(timeout, t =>
        {
            Cleanup();

            return new TimeoutException($"Timeout of {t.TotalMilliseconds} ms exceeded");
        }).ConfigureAwait(false);

        Cleanup();

        if (this.SessionClosedTask.IsFaulted)
        {
            await this.SessionClosedTask.ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override async Task<IRequest> WaitForRequestAsync(Func<IRequest, bool> predicate, WaitForOptions options = null)
    {
        var timeout = options?.Timeout ?? this.DefaultTimeout;
        var requestTcs = new TaskCompletionSource<IRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        void RequestEventListener(object sender, RequestEventArgs e)
        {
            if (predicate(e.Request))
            {
                requestTcs.TrySetResult(e.Request);
                this.FrameManager.NetworkManager.Request -= RequestEventListener;
            }
        }

        this.FrameManager.NetworkManager.Request += RequestEventListener;

        await Task.WhenAny(requestTcs.Task, this.SessionClosedTask).WithTimeout(timeout, t =>
        {
            this.FrameManager.NetworkManager.Request -= RequestEventListener;
            return new TimeoutException($"Timeout of {t.TotalMilliseconds} ms exceeded");
        }).ConfigureAwait(false);

        if (this.SessionClosedTask.IsFaulted)
        {
            await this.SessionClosedTask.ConfigureAwait(false);
        }

        return await requestTcs.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IFrame> WaitForFrameAsync(Func<IFrame, bool> predicate, WaitForOptions options = null)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var timeout = options?.Timeout ?? this.DefaultTimeout;
        var frameTcs = new TaskCompletionSource<IFrame>(TaskCreationOptions.RunContinuationsAsynchronously);

        void FrameNavigatedEventListener(object sender, FrameNavigatedEventArgs e)
        {
            if (predicate(e.Frame))
            {
                frameTcs.TrySetResult(e.Frame);
                this.FrameManager.FrameNavigated -= FrameNavigatedEventListener;
            }
        }

        void FrameAttachedEventListener(object sender, FrameEventArgs e)
        {
            if (predicate(e.Frame))
            {
                frameTcs.TrySetResult(e.Frame);
                this.FrameManager.FrameAttached -= FrameAttachedEventListener;
            }
        }

        this.FrameManager.FrameAttached += FrameAttachedEventListener;
        this.FrameManager.FrameNavigated += FrameNavigatedEventListener;

        var eventRace = Task.WhenAny(frameTcs.Task, this.SessionClosedTask).WithTimeout(timeout, t =>
        {
            this.FrameManager.FrameAttached -= FrameAttachedEventListener;
            this.FrameManager.FrameNavigated -= FrameNavigatedEventListener;
            return new TimeoutException($"Timeout of {t.TotalMilliseconds} ms exceeded");
        });

        foreach (var frame in this.Frames)
        {
            if (predicate(frame))
            {
                return frame;
            }
        }

        await eventRace.ConfigureAwait(false);

        if (this.SessionClosedTask.IsFaulted)
        {
            await this.SessionClosedTask.ConfigureAwait(false);
        }

        return await frameTcs.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task BringToFrontAsync() => this.PrimaryTargetClient.SendAsync("Page.bringToFront");

    /// <inheritdoc/>
    public override Task EmulateVisionDeficiencyAsync(VisionDeficiency type)
        => this.emulationManager.EmulateVisionDeficiencyAsync(type);

    /// <inheritdoc/>
    public override Task EmulateTimezoneAsync(string timezoneId)
        => this.emulationManager.EmulateTimezoneAsync(timezoneId);

    /// <inheritdoc/>
    public override Task EmulateIdleStateAsync(EmulateIdleOverrides overrides = null)
        => this.emulationManager.EmulateIdleStateAsync(overrides);

    /// <inheritdoc/>
    public override Task EmulateCPUThrottlingAsync(decimal? factor = null)
        => this.emulationManager.EmulateCPUThrottlingAsync(factor);

    /// <inheritdoc/>
    public override Task<IResponse> GoBackAsync(NavigationOptions options = null) => this.GoAsync(-1, options);

    /// <inheritdoc/>
    public override Task<IResponse> GoForwardAsync(NavigationOptions options = null) => this.GoAsync(1, options);

    /// <inheritdoc/>
    public override async Task<IResponse> WaitForResponseAsync(
        Func<IResponse, Task<bool>> predicate,
        WaitForOptions options = null)
    {
        var timeout = options?.Timeout ?? this.DefaultTimeout;
        var responseTcs = new TaskCompletionSource<IResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        async void ResponseEventListener(object sender, ResponseCreatedEventArgs e)
        {
            try
            {
                if (await predicate(e.Response).ConfigureAwait(false))
                {
                    responseTcs.TrySetResult(e.Response);
                    this.FrameManager.NetworkManager.Response -= ResponseEventListener;
                }
            }
            catch (Exception ex)
            {
                responseTcs.TrySetException(new PuppeteerException("Predicated failed", ex));
            }
        }

        this.FrameManager.NetworkManager.Response += ResponseEventListener;

        await Task.WhenAny(responseTcs.Task, this.SessionClosedTask).WithTimeout(timeout).ConfigureAwait(false);

        if (this.SessionClosedTask.IsFaulted)
        {
            await this.SessionClosedTask.ConfigureAwait(false);
        }

        return await responseTcs.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<FileChooser> WaitForFileChooserAsync(WaitForOptions options = null)
    {
        if (this.fileChooserInterceptors.IsEmpty)
        {
            await this.PrimaryTargetClient.SendAsync(
                "Page.setInterceptFileChooserDialog",
                new PageSetInterceptFileChooserDialog { Enabled = true, }).ConfigureAwait(false);
        }

        var timeout = options?.Timeout ?? this.TimeoutSettings.Timeout;
        var tcs = new TaskCompletionSource<FileChooser>(TaskCreationOptions.RunContinuationsAsynchronously);
        var guid = Guid.NewGuid();
        this.fileChooserInterceptors.TryAdd(guid, tcs);

        try
        {
            return await tcs.Task.WithTimeout(timeout).ConfigureAwait(false);
        }
        catch (Exception)
        {
            this.fileChooserInterceptors.TryRemove(guid, out _);
            throw;
        }
    }

    /// <inheritdoc/>
    public override Task SetBurstModeOffAsync()
    {
        this.ScreenshotBurstModeOn = false;
        if (this.ScreenshotBurstModeOptions != null)
        {
            return this.ResetBackgroundColorAndViewportAsync(this.ScreenshotBurstModeOptions);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task SetExtraHttpHeadersAsync(Dictionary<string, string> headers)
    {
        if (headers == null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        return this.FrameManager.NetworkManager.SetExtraHTTPHeadersAsync(headers);
    }

    /// <inheritdoc/>
    public override Task AuthenticateAsync(Credentials credentials) =>
        this.FrameManager.NetworkManager.AuthenticateAsync(credentials);

    /// <inheritdoc/>
    public override async Task CloseAsync(PageCloseOptions options = null)
    {
        if (this.Client?.Connection?.IsClosed ?? true)
        {
            this.logger.LogWarning("Protocol error: Connection closed. Most likely the page has been closed.");
            return;
        }

        var runBeforeUnload = options?.RunBeforeUnload ?? false;

        if (runBeforeUnload)
        {
            await this.PrimaryTargetClient.SendAsync("Page.close").ConfigureAwait(false);
        }
        else
        {
            await this.PrimaryTargetClient.Connection
                .SendAsync("Target.closeTarget", new TargetCloseTargetRequest { TargetId = this.Target.TargetId, })
                .ConfigureAwait(false);

            // Puppeteer waits for Target.CloseTask. But I found some race condition where IsClose didn't get set to true.
            // So I'm waiting for the task that set IsClose to true.
            await this.closedFinishedTask.ConfigureAwait(false);
        }
    }

    internal static async Task<Page> CreateAsync(
        CdpCDPSession client,
        CdpTarget target,
        bool acceptInsecureCerts,
        ViewPortOptions defaultViewPort,
        TaskQueue screenshotTaskQueue)
    {
        var page = new CdpPage(client, target, screenshotTaskQueue, acceptInsecureCerts);

        try
        {
            await page.InitializeAsync().ConfigureAwait(false);

            if (defaultViewPort != null)
            {
                await page.SetViewportAsync(defaultViewPort).ConfigureAwait(false);
            }

            return page;
        }
        catch
        {
            await page.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    internal static decimal ConvertPrintParameterToInches(object parameter)
    {
        if (parameter == null)
        {
            return 0;
        }

        decimal pixels;
        if (parameter is decimal or int)
        {
            pixels = Convert.ToDecimal(parameter, CultureInfo.CurrentCulture);
        }
        else
        {
            var text = parameter.ToString();
            var unit = text.Length > 2 ? text.Substring(text.Length - 2).ToLower(CultureInfo.CurrentCulture) : string.Empty;
            string valueText;
            if (GetPixels(unit) is { })
            {
                valueText = text.Substring(0, text.Length - 2);
            }
            else
            {
                // In case of unknown unit try to parse the whole parameter as number of pixels.
                // This is consistent with phantom's paperSize behavior.
                unit = "px";
                valueText = text;
            }

            if (decimal.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var number))
            {
                pixels = number * GetPixels(unit).Value;
            }
            else
            {
                throw new ArgumentException($"Failed to parse parameter value: '{text}'", nameof(parameter));
            }
        }

        return pixels / 96;
    }

    /// <inheritdoc />
    protected override async Task ExposeFunctionAsync(string name, Delegate puppeteerFunction)
    {
        if (!this.bindings.TryAdd(name, new Binding(name, puppeteerFunction)))
        {
            throw new PuppeteerException(
                $"Failed to add page binding with name {name}: window['{name}'] already exists!");
        }

        var expression = BindingUtils.PageBindingInitString("exposedFun", name);
        await this.PrimaryTargetClient.SendAsync("Runtime.addBinding", new RuntimeAddBindingRequest { Name = name })
            .ConfigureAwait(false);
        var functionInfo = await this.PrimaryTargetClient
            .SendAsync<PageAddScriptToEvaluateOnNewDocumentResponse>(
                "Page.addScriptToEvaluateOnNewDocument",
                new PageAddScriptToEvaluateOnNewDocumentRequest { Source = expression, }).ConfigureAwait(false);

        this.exposedFunctions.TryAdd(name, functionInfo.Identifier);

        await Task.WhenAll(this.Frames.Select(
                frame =>
                {
                    // If a frame has not started loading, it might never start. Rely on
                    // addScriptToEvaluateOnNewDocument in that case.
                    if (frame != this.MainFrame && !((Frame)frame).HasStartedLoading)
                    {
                        return Task.CompletedTask;
                    }

                    return frame
                        .EvaluateExpressionAsync(expression)
                        .ContinueWith(
                            task =>
                            {
                                if (task.IsFaulted && task.Exception != null)
                                {
                                    this.logger.LogError(task.Exception.ToString());
                                }
                            },
                            TaskScheduler.Default);
                }))
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task<byte[]> PdfInternalAsync(string file, PdfOptions options)
    {
        var paperWidth = PaperFormat.Letter.Width;
        var paperHeight = PaperFormat.Letter.Height;

        Debug.Assert(options != null, nameof(options) + " != null");

        if (options.Format != null)
        {
            paperWidth = options.Format.Width;
            paperHeight = options.Format.Height;
        }
        else
        {
            if (options.Width != null)
            {
                paperWidth = ConvertPrintParameterToInches(options.Width);
            }

            if (options.Height != null)
            {
                paperHeight = ConvertPrintParameterToInches(options.Height);
            }
        }

        var marginTop = ConvertPrintParameterToInches(options.MarginOptions.Top);
        var marginLeft = ConvertPrintParameterToInches(options.MarginOptions.Left);
        var marginBottom = ConvertPrintParameterToInches(options.MarginOptions.Bottom);
        var marginRight = ConvertPrintParameterToInches(options.MarginOptions.Right);

        if (options.Outline)
        {
            options.Tagged = true;
        }

        if (options.OmitBackground)
        {
            await this.emulationManager.SetTransparentBackgroundColorAsync().ConfigureAwait(false);
        }

        await this.FrameManager.MainFrame.IsolatedRealm.EvaluateExpressionAsync("() => documents.fonts.ready")
            .WithTimeout(this.TimeoutSettings.Timeout).ConfigureAwait(false);

        var result = await this.PrimaryTargetClient.SendAsync<PagePrintToPDFResponse>(
            "Page.printToPDF",
            new PagePrintToPDFRequest
            {
                TransferMode = "ReturnAsStream",
                Landscape = options.Landscape,
                DisplayHeaderFooter = options.DisplayHeaderFooter,
                HeaderTemplate = options.HeaderTemplate,
                FooterTemplate = options.FooterTemplate,
                PrintBackground = options.PrintBackground,
                Scale = options.Scale,
                PaperWidth = paperWidth,
                PaperHeight = paperHeight,
                MarginTop = marginTop,
                MarginBottom = marginBottom,
                MarginLeft = marginLeft,
                MarginRight = marginRight,
                PageRanges = options.PageRanges,
                PreferCSSPageSize = options.PreferCSSPageSize,
                GenerateTaggedPDF = options.Tagged,
                GenerateDocumentOutline = options.Outline,
            }).ConfigureAwait(false);

        if (options.OmitBackground)
        {
            await this.emulationManager.ResetDefaultBackgroundColorAsync().ConfigureAwait(false);
        }

        return await ProtocolStreamReader.ReadProtocolStreamByteAsync(this.Client, result.Stream, file)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task<string> PerformScreenshotAsync(ScreenshotType type, ScreenshotOptions options)
    {
        var stack = new DisposableTasksStack();
        await using (stack.ConfigureAwait(false))
        {
            Debug.Assert(options != null, nameof(options) + " != null");

            var clip = options.Clip;
            var captureBeyondViewport = options.CaptureBeyondViewport;

            if (this.Browser.BrowserType != SupportedBrowser.Firefox &&
                options.OmitBackground &&
                (type == ScreenshotType.Png || type == ScreenshotType.Webp))
            {
                await this.emulationManager.SetTransparentBackgroundColorAsync().ConfigureAwait(false);
                stack.Defer(() => this.emulationManager.ResetDefaultBackgroundColorAsync());
            }

            if (clip != null && !captureBeyondViewport)
            {
                var viewport = await this.FrameManager.MainFrame.IsolatedRealm.EvaluateFunctionAsync<BoundingBox>(
                    @"() => {
                        const {
                            height,
                            pageLeft: x,
                            pageTop: y,
                            width,
                        } = window.visualViewport;
                        return {x, y, height, width};
                    }").ConfigureAwait(false);

                clip = this.GetIntersectionRect(clip, viewport);
            }

            var screenMessage = new PageCaptureScreenshotRequest
            {
                Format = type.ToString().ToLower(CultureInfo.CurrentCulture),
                CaptureBeyondViewport = captureBeyondViewport,
                FromSurface = options.FromSurface,
                OptimizeForSpeed = options.OptimizeForSpeed,
            };

            if (options.Quality.HasValue)
            {
                screenMessage.Quality = options.Quality.Value;
            }

            if (clip != null)
            {
                screenMessage.Clip = clip;
            }

            var result = await this.PrimaryTargetClient
                .SendAsync<PageCaptureScreenshotResponse>("Page.captureScreenshot", screenMessage)
                .ConfigureAwait(false);

            return result.Data;
        }
    }

    private static decimal? GetPixels(string unit) => unit switch
    {
        "px" => 1,
        "in" => 96,
        "cm" => 37.8m,
        "mm" => 3.78m,
        _ => null,
    };

    private void SetupPrimaryTargetListeners()
    {
        this.PrimaryTargetClient.Ready += this.OnAttachedToTarget;
        this.PrimaryTargetClient.MessageReceived += this.Client_MessageReceived;
    }

    private void OnAttachedToTarget(object sender, SessionEventArgs e)
    {
        var session = e.Session as CDPSession;
        Debug.Assert(session != null, nameof(session) + " != null");
        this.FrameManager.OnAttachedToTarget(new TargetChangedArgs { Target = session.Target });

        if (session.Target.Type == TargetType.Worker)
        {
            var worker = new CdpWebWorker(
                session,
                session.Target.Url,
                session.Target.TargetId,
                session.Target.TargetInfo.Type,
                this.AddConsoleMessageAsync,
                this.HandleException);
            this.workers[session.Id] = worker;
            this.OnWorkerCreated(worker);
        }

        session.Ready += this.OnAttachedToTarget;
    }

    private async void Client_MessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            switch (e.MessageID)
            {
                case "Page.domContentEventFired":
                    this.OnDOMContentLoaded();
                    break;
                case "Page.loadEventFired":
                    this.OnLoad();
                    break;
                case "Runtime.consoleAPICalled":
                    await this.OnConsoleAPIAsync(e.MessageData.ToObject<PageConsoleResponse>())
                        .ConfigureAwait(false);
                    break;
                case "Page.javascriptDialogOpening":
                    this.OnDialog(e.MessageData.ToObject<PageJavascriptDialogOpeningResponse>());
                    break;
                case "Runtime.exceptionThrown":
                    this.HandleException(e.MessageData.ToObject<RuntimeExceptionThrownResponse>().ExceptionDetails);
                    break;
                case "Inspector.targetCrashed":
                    this.OnTargetCrashed();
                    break;
                case "Performance.metrics":
                    this.EmitMetrics(e.MessageData.ToObject<PerformanceMetricsResponse>());
                    break;
                case "Log.entryAdded":
                    await this.OnLogEntryAddedAsync(e.MessageData.ToObject<LogEntryAddedResponse>())
                        .ConfigureAwait(false);
                    break;
                case "Runtime.bindingCalled":
                    await this.OnBindingCalledAsync(e.MessageData.ToObject<BindingCalledResponse>())
                        .ConfigureAwait(false);
                    break;
                case "Page.fileChooserOpened":
                    await this.OnFileChooserAsync(e.MessageData.ToObject<PageFileChooserOpenedResponse>())
                        .ConfigureAwait(false);
                    break;
            }
        }
        catch (Exception ex)
        {
            var message = $"Page failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
            this.PrimaryTargetClient.Close(message);
        }
    }

    private async Task OnActivationAsync(CdpCDPSession newSession)
    {
        try
        {
            this.PrimaryTargetClient = newSession;
            this.PrimaryTarget = (CdpTarget)this.PrimaryTargetClient.Target;
            ((CdpKeyboard)this.Keyboard).UpdateClient(this.Client);
            ((CdpMouse)this.Mouse).UpdateClient(this.Client);
            ((CdpTouchscreen)this.Touchscreen).UpdateClient(this.Client);
            this.Accessibility.UpdateClient(this.Client);
            this.emulationManager.UpdateClient(this.Client);
            this.Tracing.UpdateClient(this.Client);
            this.Coverage.UpdateClient(this.Client);
            await this.FrameManager.SwapFrameTreeAsync(this.Client).ConfigureAwait(false);
            this.SetupPrimaryTargetListeners();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to activate primary target");
        }
    }

    private async Task OnSecondaryTargetAsync(CDPSession session)
    {
        if (session.Target.TargetInfo.Subtype != "prerender")
        {
            return;
        }

        try
        {
            await this.FrameManager.RegisterSpeculativeSessionAsync(session).ConfigureAwait(false);
            await this.emulationManager.RegisterSpeculativeSessionAsync(session).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to register speculative session");
        }
    }

    private void OnDetachedFromTarget(object sender, TargetChangedArgs e)
    {
        var sessionId = e.Target.Session?.Id;
        if (sessionId != null && this.workers.TryRemove(sessionId, out var worker))
        {
            this.OnWorkerDestroyed(worker);
        }
    }

    private Task OnConsoleAPIAsync(PageConsoleResponse message)
    {
        if (message.ExecutionContextId == 0)
        {
            return Task.CompletedTask;
        }

        var ctx = this.FrameManager.ExecutionContextById(message.ExecutionContextId, this.Client);

        if (ctx == null)
        {
            this.logger.LogError($"ExecutionContext not found from message.");
            return Task.CompletedTask;
        }

        var values = message.Args.Select(ctx.CreateJSHandle).ToArray();

        return this.AddConsoleMessageAsync(message.Type, values, message.StackTrace);
    }

    private async Task AddConsoleMessageAsync(ConsoleType type, IJSHandle[] values, StackTrace stackTrace)
    {
        if (this.HasConsoleEventListeners)
        {
            await Task.WhenAll(values.Select(v =>
                RemoteObjectHelper.ReleaseObjectAsync(this.Client, v.RemoteObject, this.logger))).ConfigureAwait(false);
            return;
        }

        var tokens = values.Select(i =>
            i.RemoteObject.ObjectId != null || i.RemoteObject.Type == RemoteObjectType.Object
                ? i.ToString()
                : RemoteObjectHelper.ValueFromRemoteObject<object>(i.RemoteObject)?.ToString() ?? "null");

        var location = new ConsoleMessageLocation();
        if (stackTrace?.CallFrames?.Length > 0)
        {
            var callFrame = stackTrace.CallFrames[0];
            location.URL = callFrame.URL;
            location.LineNumber = callFrame.LineNumber;
            location.ColumnNumber = callFrame.ColumnNumber;
        }

        var consoleMessage = new ConsoleMessage(type, string.Join(" ", tokens), values, location);
        this.OnConsole(new ConsoleEventArgs(consoleMessage));
    }

    private void EmitMetrics(PerformanceMetricsResponse metrics)
        => this.OnMetrics(new MetricEventArgs(metrics.Title, this.BuildMetricsObject(metrics.Metrics)));

    private void HandleException(EvaluateExceptionResponseDetails exceptionDetails)
        => this.OnPageError(new PageErrorEventArgs(this.GetExceptionMessage(exceptionDetails)));

    private Dictionary<string, decimal> BuildMetricsObject(List<Metric> metrics)
    {
        var result = new Dictionary<string, decimal>();

        foreach (var item in metrics)
        {
            if (SupportedMetrics.Contains(item.Name))
            {
                result.Add(item.Name, item.Value);
            }
        }

        return result;
    }

    private string GetExceptionMessage(EvaluateExceptionResponseDetails exceptionDetails)
    {
        if (exceptionDetails.Exception != null)
        {
            return exceptionDetails.Exception.Description;
        }

        var message = exceptionDetails.Text;
        if (exceptionDetails.StackTrace == null)
        {
            return message;
        }

        foreach (var callFrame in exceptionDetails.StackTrace.CallFrames)
        {
            var location = $"{callFrame.Url}:{callFrame.LineNumber}:{callFrame.ColumnNumber}";
            var functionName = callFrame.FunctionName ?? "<anonymous>";
            message += $"\n at {functionName} ({location})";
        }

        return message;
    }

    private void OnTargetCrashed()
    {
        if (!this.HasErrorEventListeners)
        {
            throw new TargetCrashedException();
        }

        this.OnError(new ErrorEventArgs("Page crashed!"));
    }

    private async Task OnLogEntryAddedAsync(LogEntryAddedResponse e)
    {
        if (e.Entry.Args != null)
        {
            foreach (var arg in e.Entry.Args)
            {
                await RemoteObjectHelper.ReleaseObjectAsync(this.PrimaryTargetClient, arg, this.logger)
                    .ConfigureAwait(false);
            }
        }

        if (e.Entry.Source != LogSource.Worker)
        {
            this.OnConsole(new ConsoleEventArgs(new ConsoleMessage(
                e.Entry.Level,
                e.Entry.Text,
                null,
                new ConsoleMessageLocation { URL = e.Entry.URL, LineNumber = e.Entry.LineNumber, })));
        }
    }

    private async Task OnBindingCalledAsync(BindingCalledResponse e)
    {
        if (e.BindingPayload.Type != "exposedFun" || !this.bindings.ContainsKey(e.BindingPayload.Name))
        {
            return;
        }

        var context = this.FrameManager.GetExecutionContextById(e.ExecutionContextId, this.Client);

        await BindingUtils.ExecuteBindingAsync(context, e, this.bindings).ConfigureAwait(false);
    }

    private async Task OnFileChooserAsync(PageFileChooserOpenedResponse e)
    {
        if (this.fileChooserInterceptors.IsEmpty)
        {
            try
            {
                await this.PrimaryTargetClient.SendAsync(
                        "Page.handleFileChooser",
                        new PageHandleFileChooserRequest { Action = FileChooserAction.Fallback, })
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.ToString());
            }
        }

        var frame = await this.FrameManager.FrameTree.GetFrameAsync(e.FrameId).ConfigureAwait(false);
        var element = await frame.MainWorld.AdoptBackendNodeAsync(e.BackendNodeId).ConfigureAwait(false);
        var fileChooser = new FileChooser(element, e);
        while (!this.fileChooserInterceptors.IsEmpty)
        {
            var key = this.fileChooserInterceptors.FirstOrDefault().Key;

            if (this.fileChooserInterceptors.TryRemove(key, out var tcs))
            {
                tcs.TrySetResult(fileChooser);
            }
        }
    }

    private Clip GetIntersectionRect(Clip clip, BoundingBox viewport)
    {
        var x = Math.Max(clip.X, viewport.X);
        var y = Math.Max(clip.Y, viewport.Y);

        return new Clip()
        {
            X = x,
            Y = y,
            Width = Math.Min(clip.X + clip.Width, viewport.X + viewport.Width) - x,
            Height = Math.Min(clip.Y + clip.Height, viewport.Y + viewport.Height) - y,
        };
    }

    private async Task InitializeAsync()
    {
        await this.FrameManager.InitializeAsync(this.PrimaryTargetClient).ConfigureAwait(false);

        await Task.WhenAll(
            this.PrimaryTargetClient.SendAsync("Performance.enable"),
            this.PrimaryTargetClient.SendAsync("Log.enable")).ConfigureAwait(false);
    }

    private async Task<IResponse> GoAsync(int delta, NavigationOptions options)
    {
        var history = await this.PrimaryTargetClient
            .SendAsync<PageGetNavigationHistoryResponse>("Page.getNavigationHistory").ConfigureAwait(false);

        if (history.Entries.Count <= history.CurrentIndex + delta || history.CurrentIndex + delta < 0)
        {
            return null;
        }

        var entry = history.Entries[history.CurrentIndex + delta];
        var waitTask = this.WaitForNavigationAsync(options);

        await Task.WhenAll(
            waitTask,
            this.PrimaryTargetClient.SendAsync(
                "Page.navigateToHistoryEntry",
                new PageNavigateToHistoryEntryRequest { EntryId = entry.Id, })).ConfigureAwait(false);

        return waitTask.Result;
    }

    private Task ResetBackgroundColorAndViewportAsync(ScreenshotOptions options)
    {
        var omitBackgroundTask = options is { OmitBackground: true, Type: ScreenshotType.Png }
            ? this.emulationManager.ResetDefaultBackgroundColorAsync()
            : Task.CompletedTask;
        var setViewPortTask = (options?.FullPage == true && this.Viewport != null)
            ? this.SetViewportAsync(this.Viewport)
            : Task.CompletedTask;
        return Task.WhenAll(omitBackgroundTask, setViewPortTask);
    }

    private void AttachExistingTargets()
    {
        List<ITarget> queue = [];
        queue.AddRange(this.targetManager.GetChildTargets(this.PrimaryTarget));

        for (var idx = 0; idx < queue.Count; idx++)
        {
            var next = (CdpTarget)queue[idx];
            var session = next.Session;

            if (session != null)
            {
                this.OnAttachedToTarget(this, new SessionEventArgs(session));
            }

            queue.AddRange(this.targetManager.GetChildTargets(next));
        }
    }
}
