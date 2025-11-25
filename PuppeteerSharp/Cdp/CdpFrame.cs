// <copyright file="CdpFrame.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;

/// <inheritdoc />
public class CdpFrame : Frame
{
    private const string RefererHeaderName = "referer";

    internal CdpFrame(FrameManager frameManager, string frameId, string parentFrameId, CDPSession client)
    {
        this.FrameManager = frameManager;
        this.Id = frameId;
        this.Client = client;
        this.ParentId = parentFrameId;

        this.UpdateClient(client);

        this.FrameSwappedByActivation += (_, _) =>
        {
            // Emulate loading process for swapped frames.
            this.OnLoadingStarted();
            this.OnLoadingStopped();
        };

        this.Logger = client.Connection.LoggerFactory.CreateLogger<Frame>();
    }

    /// <inheritdoc />
    public sealed override CDPSession Client { get; protected set; }

    /// <inheritdoc/>
    public override IPage Page => this.FrameManager.Page;

    /// <inheritdoc/>
    public override IReadOnlyCollection<IFrame> ChildFrames => this.FrameManager.FrameTree.GetChildFrames(this.Id);

    internal FrameManager FrameManager { get; }

    internal CdpPage CdpPage => this.Page as CdpPage;

    internal override Frame ParentFrame => this.FrameManager.FrameTree.GetParentFrame(this.Id);

    /// <inheritdoc/>
    public override async Task<IResponse> GoToAsync(string url, NavigationOptions options)
    {
        var ensureNewDocumentNavigation = false;

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var referrer = string.IsNullOrEmpty(options.Referer)
            ? this.FrameManager.NetworkManager.ExtraHTTPHeaders?.GetValue(RefererHeaderName)
            : options.Referer;
        var referrerPolicy = string.IsNullOrEmpty(options.ReferrerPolicy)
            ? this.FrameManager.NetworkManager.ExtraHTTPHeaders?.GetValue("referer-policy")
            : options.ReferrerPolicy;
        var timeout = options.Timeout ?? this.FrameManager.TimeoutSettings.NavigationTimeout;

        using var watcher = new LifecycleWatcher(this.FrameManager.NetworkManager, this, options.WaitUntil, timeout);
        try
        {
            var navigateTask = NavigateAsync();
            var task = await Task.WhenAny(
                watcher.TerminationTask,
                navigateTask).ConfigureAwait(false);

            await task.ConfigureAwait(false);

            task = await Task.WhenAny(
                    watcher.TerminationTask,
                    ensureNewDocumentNavigation ? watcher.NewDocumentNavigationTask : watcher.SameDocumentNavigationTask)
                .ConfigureAwait(false);

            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new NavigationException(ex.Message, ex);
        }

        return watcher.NavigationResponse;

        async Task NavigateAsync()
        {
            var response = await this.Client.SendAsync<PageNavigateResponse>("Page.navigate", new PageNavigateRequest
            {
                Url = url,
                Referrer = referrer ?? string.Empty,
                ReferrerPolicy = ReferrerPolicyToProtocol(referrerPolicy),
                FrameId = this.Id,
            }).ConfigureAwait(false);

            ensureNewDocumentNavigation = !string.IsNullOrEmpty(response.LoaderId);

            if (!string.IsNullOrEmpty(response.ErrorText) &&
                response.ErrorText != "net::ERR_HTTP_RESPONSE_CODE_FAILURE")
            {
                throw new NavigationException(response.ErrorText, url);
            }
        }
    }

    /// <inheritdoc/>
    public override async Task<IResponse> WaitForNavigationAsync(NavigationOptions options = null)
    {
        var timeout = options?.Timeout ?? this.FrameManager.TimeoutSettings.NavigationTimeout;
        using var watcher = new LifecycleWatcher(this.FrameManager.NetworkManager, this, options?.WaitUntil, timeout);
        var raceTask = await Task.WhenAny(
        [
            watcher.NewDocumentNavigationTask,
            .. options?.IgnoreSameDocumentNavigation == true
                ? Array.Empty<Task>()
                : [watcher.SameDocumentNavigationTask],
            watcher.TerminationTask,
        ]).ConfigureAwait(false);

        await raceTask.ConfigureAwait(false);

        return watcher.NavigationResponse;
    }

    /// <inheritdoc/>
    public override async Task SetContentAsync(string html, NavigationOptions options = null)
    {
        var waitUntil = options?.WaitUntil ?? new[] { WaitUntilNavigation.Load };
        var timeout = options?.Timeout ?? this.FrameManager.TimeoutSettings.NavigationTimeout;

        // We rely upon the fact that document.open() will reset frame lifecycle with "init"
        // lifecycle event. @see https://crrev.com/608658
        await this.IsolatedRealm.EvaluateFunctionAsync(
            @"html => {
                    document.open();
                    document.write(html);
                    document.close();
                }",
            html).ConfigureAwait(false);

        using var watcher = new LifecycleWatcher(this.FrameManager.NetworkManager, this, waitUntil, timeout);
        var watcherTask = await Task.WhenAny(
            watcher.TerminationTask,
            watcher.LifecycleTask).ConfigureAwait(false);

        await watcherTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IElementHandle> AddStyleTagAsync(AddTagOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrEmpty(options.Url) && string.IsNullOrEmpty(options.Path) &&
            string.IsNullOrEmpty(options.Content))
        {
            throw new ArgumentException("Provide options with a `Url`, `Path` or `Content` property");
        }

        var content = options.Content;

        if (!string.IsNullOrEmpty(options.Path))
        {
            content = await AsyncFileHelper.ReadAllText(options.Path).ConfigureAwait(false);
            content += "//# sourceURL=" + options.Path.Replace("\n", string.Empty);
        }

        var handle = await this.IsolatedRealm.EvaluateFunctionHandleAsync(
            @"async (puppeteerUtil, url, id, type, content) => {
                  const createDeferredPromise = puppeteerUtil.createDeferredPromise;
                  const promise = createDeferredPromise();
                  let element;
                  if (!url) {
                    element = document.createElement('style');
                    element.appendChild(document.createTextNode(content));
                  } else {
                    const link = document.createElement('link');
                    link.rel = 'stylesheet';
                    link.href = url;
                    element = link;
                  }
                  element.addEventListener(
                    'load',
                    () => {
                      promise.resolve();
                    },
                    {once: true}
                  );
                  element.addEventListener(
                    'error',
                    event => {
                      promise.reject(
                        new Error(
                          event.message ?? 'Could not load style'
                        )
                      );
                    },
                    {once: true}
                  );
                  document.head.appendChild(element);
                  await promise;
                  return element;
                }",
            new LazyArg(async context => await context.GetPuppeteerUtilAsync().ConfigureAwait(false)),
            options.Url,
            options.Id,
            options.Type,
            content).ConfigureAwait(false);

        return (await this.MainRealm.TransferHandleAsync(handle).ConfigureAwait(false)) as IElementHandle;
    }

    /// <inheritdoc/>
    public override async Task<IElementHandle> AddScriptTagAsync(AddTagOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrEmpty(options.Url) && string.IsNullOrEmpty(options.Path) &&
            string.IsNullOrEmpty(options.Content))
        {
            throw new ArgumentException("Provide options with a `Url`, `Path` or `Content` property");
        }

        var content = options.Content;

        if (!string.IsNullOrEmpty(options.Path))
        {
            content = await AsyncFileHelper.ReadAllText(options.Path).ConfigureAwait(false);
            content += "//# sourceURL=" + options.Path.Replace("\n", string.Empty);
        }

        var handle = await this.IsolatedRealm.EvaluateFunctionHandleAsync(
            @"async (puppeteerUtil, url, id, type, content) => {
                  const createDeferredPromise = puppeteerUtil.createDeferredPromise;
                  const promise = createDeferredPromise();
                  const script = document.createElement('script');
                  script.type = type;
                  script.text = content;
                  if (url) {
                    script.src = url;
                    script.addEventListener(
                      'load',
                      () => {
                        return promise.resolve();
                      },
                      {once: true}
                    );
                    script.addEventListener(
                      'error',
                      event => {
                        promise.reject(
                          new Error(event.message ?? 'Could not load script')
                        );
                      },
                      {once: true}
                    );
                  } else {
                    promise.resolve();
                  }
                  if (id) {
                    script.id = id;
                  }
                  document.head.appendChild(script);
                  await promise;
                  return script;
                }",
            new LazyArg(async context => await context.GetPuppeteerUtilAsync().ConfigureAwait(false)),
            options.Url,
            options.Id,
            options.Type,
            content).ConfigureAwait(false);

        return (await this.MainRealm.TransferHandleAsync(handle).ConfigureAwait(false)) as IElementHandle;
    }

    internal void UpdateClient(CDPSession client, bool keepWorlds = false)
    {
        this.Client = client;

        if (!keepWorlds)
        {
            this.MainWorld?.ClearContext();
            this.PuppeteerWorld?.ClearContext();

            this.MainRealm = new IsolatedWorld(
                this,
                null,
                this.CdpPage.TimeoutSettings,
                true);

            this.IsolatedRealm = new IsolatedWorld(
                this,
                null,
                this.CdpPage.TimeoutSettings,
                false);
        }
        else
        {
            this.MainWorld.FrameUpdated();
            this.PuppeteerWorld.FrameUpdated();
        }
    }

    /// <inheritdoc />
    protected internal override DeviceRequestPromptManager GetDeviceRequestPromptManager()
        => this.FrameManager.GetDeviceRequestPromptManager(this.Client);

    // See https://chromedevtools.github.io/devtools-protocol/tot/Page/#type-ReferrerPolicy.
    private static string ReferrerPolicyToProtocol(string referrerPolicy)
    {
        // Transform kebab-case to camelCase
        return string.IsNullOrEmpty(referrerPolicy) ? null :
            Regex.Replace(referrerPolicy, "-(.)", match => match.Groups[1].Value.ToUpperInvariant());
    }
}
