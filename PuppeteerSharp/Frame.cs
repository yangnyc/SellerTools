// <copyright file="Frame.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Input;

    /// <inheritdoc cref="PuppeteerSharp.IFrame" />
    public abstract class Frame : IFrame, IEnvironment
    {
        private Task<ElementHandle> documentTask;

        /// <inheritdoc />
        public event EventHandler FrameSwappedByActivation;

        internal event EventHandler FrameDetached;

        internal event EventHandler<FrameNavigatedEventArgs> FrameNavigated;

        internal event EventHandler FrameNavigatedWithinDocument;

        internal event EventHandler LifecycleEvent;

        internal event EventHandler FrameSwapped;

        /// <inheritdoc/>
        public abstract IReadOnlyCollection<IFrame> ChildFrames { get; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Url { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public bool Detached { get; private set; }

        /// <inheritdoc/>
        public abstract IPage Page { get; }

        /// <inheritdoc/>
        IFrame IFrame.ParentFrame => this.ParentFrame;

        /// <inheritdoc/>
        public string Id { get; internal set; }

        /// <inheritdoc/>
        public abstract CDPSession Client { get; protected set; }

        /// <inheritdoc/>
        Realm IEnvironment.MainRealm => this.MainRealm;

        internal string ParentId { get; init; }

        internal string LoaderId { get; private set; } = string.Empty;

        internal List<string> LifecycleEvents { get; } = new();

        internal Realm MainRealm { get; set; }

        internal Realm IsolatedRealm { get; set; }

        internal IsolatedWorld MainWorld => this.MainRealm as IsolatedWorld;

        internal IsolatedWorld PuppeteerWorld => this.IsolatedRealm as IsolatedWorld;

        internal bool HasStartedLoading { get; private set; }

        internal abstract Frame ParentFrame { get; }

        /// <summary>
        /// Gets logger.
        /// </summary>
        protected ILogger Logger { get; init; }

        /// <inheritdoc/>
        public abstract Task<IResponse> GoToAsync(string url, NavigationOptions options);

        /// <inheritdoc/>
        public Task<IResponse> GoToAsync(string url, int? timeout = null, WaitUntilNavigation[] waitUntil = null)
            => this.GoToAsync(url, new NavigationOptions { Timeout = timeout, WaitUntil = waitUntil });

        /// <inheritdoc/>
        public abstract Task<IResponse> WaitForNavigationAsync(NavigationOptions options = null);

        /// <inheritdoc/>
        public Task<JsonElement?> EvaluateExpressionAsync(string script) => this.MainRealm.EvaluateExpressionAsync(script);

        /// <inheritdoc/>
        public Task<T> EvaluateExpressionAsync<T>(string script) => this.MainRealm.EvaluateExpressionAsync<T>(script);

        /// <inheritdoc/>
        public Task<JsonElement?> EvaluateFunctionAsync(string script, params object[] args) => this.MainRealm.EvaluateFunctionAsync(script, args);

        /// <inheritdoc/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args) => this.MainRealm.EvaluateFunctionAsync<T>(script, args);

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateExpressionHandleAsync(string script) => this.MainRealm.EvaluateExpressionHandleAsync(script);

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateFunctionHandleAsync(string function, params object[] args) => this.MainRealm.EvaluateFunctionHandleAsync(function, args);

        /// <inheritdoc/>
        public async Task<IElementHandle> WaitForSelectorAsync(string selector, WaitForSelectorOptions options = null)
        {
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var (updatedSelector, queryHandler) = this.Client.Connection.CustomQuerySelectorRegistry.GetQueryHandlerAndSelector(selector);
            return await queryHandler.WaitForAsync(this, null, updatedSelector, options).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<IElementHandle> WaitForXPathAsync(string xpath, WaitForSelectorOptions options = null)
        {
            if (string.IsNullOrEmpty(xpath))
            {
                throw new ArgumentNullException(nameof(xpath));
            }

            if (xpath.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                xpath = $".{xpath}";
            }

            return this.WaitForSelectorAsync($"xpath/{xpath}", options);
        }

        /// <inheritdoc/>
        public Task<IJSHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options, params object[] args)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return this.MainRealm.WaitForFunctionAsync(script, options, args);
        }

        /// <inheritdoc/>
        public Task<IJSHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return this.MainRealm.WaitForExpressionAsync(script, options);
        }

        /// <inheritdoc/>
        public async Task<string[]> SelectAsync(string selector, params string[] values)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            return await handle.SelectAsync(values).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IJSHandle> QuerySelectorAllHandleAsync(string selector)
        {
            var document = await this.GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAllHandleAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IElementHandle> QuerySelectorAsync(string selector)
        {
            var document = await this.GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IElementHandle[]> QuerySelectorAllAsync(string selector)
        {
            var document = await this.GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAllAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IElementHandle[]> XPathAsync(string expression)
        {
            var document = await this.GetDocumentAsync().ConfigureAwait(false);
            return await document.XPathAsync(expression).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<DeviceRequestPrompt> WaitForDevicePromptAsync(WaitForOptions options = default)
            => this.GetDeviceRequestPromptManager().WaitForDevicePromptAsync(options);

        /// <inheritdoc/>
        public abstract Task<IElementHandle> AddStyleTagAsync(AddTagOptions options);

        /// <inheritdoc/>
        public abstract Task<IElementHandle> AddScriptTagAsync(AddTagOptions options);

        /// <inheritdoc/>
        public Task<string> GetContentAsync(GetContentOptions options = null)
            => this.EvaluateFunctionAsync<string>(
                @"(replaceLoneSurrogates) => {
                    let content = '';
                    for (const node of document.childNodes) {
                        switch (node) {
                        case document.documentElement:
                            content += document.documentElement.outerHTML;
                            break;
                        default:
                            content += new XMLSerializer().serializeToString(node);
                            break;
                        }
                    }

                    return replaceLoneSurrogates ? content.toWellFormed() : content;
                }",
                options?.ReplaceLoneSurrogates ?? false);

        /// <inheritdoc/>
        public abstract Task SetContentAsync(string html, NavigationOptions options = null);

        /// <inheritdoc/>
        public Task<string> GetTitleAsync() => this.IsolatedRealm.EvaluateExpressionAsync<string>("document.title");

        /// <inheritdoc/>
        public async Task ClickAsync(string selector, ClickOptions options = null)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.ClickAsync(options).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task HoverAsync(string selector)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.HoverAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task FocusAsync(string selector)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.FocusAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task TapAsync(string selector)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);
            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.TapAsync().ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task TypeAsync(string selector, string text, TypeOptions options = null)
        {
            var handle = await this.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.TypeAsync(text, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ElementHandle> FrameElementAsync()
        {
            var parentFrame = this.ParentFrame;
            if (parentFrame == null)
            {
                return null;
            }

            var list = await parentFrame.IsolatedRealm.EvaluateFunctionHandleAsync(@"() => {
                return document.querySelectorAll('iframe, frame');
            }").ConfigureAwait(false);

            await foreach (var iframe in list.TransposeIterableHandleAsync().ConfigureAwait(false))
            {
                var frame = await iframe.ContentFrameAsync().ConfigureAwait(false);
                if (frame?.Id == this.Id)
                {
                    return iframe as ElementHandle;
                }

                try
                {
                    await iframe.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    this.Logger.LogWarning("FrameElementAsync: Error disposing iframe");
                }
            }

            return null;
        }

        internal void ClearDocumentHandle() => this.documentTask = null;

        internal void OnLoadingStarted() => this.HasStartedLoading = true;

        internal void OnLoadingStopped()
        {
            this.LifecycleEvents.Add("DOMContentLoaded");
            this.LifecycleEvents.Add("load");
            this.LifecycleEvent?.Invoke(this, EventArgs.Empty);
        }

        internal void OnLifecycleEvent(string loaderId, string name)
        {
            if (name == "init")
            {
                this.LoaderId = loaderId;
                this.LifecycleEvents.Clear();
            }

            this.LifecycleEvents.Add(name);
            this.LifecycleEvent?.Invoke(this, EventArgs.Empty);
        }

        internal void Navigated(FramePayload framePayload)
        {
            this.Name = framePayload.Name ?? string.Empty;
            this.Url = framePayload.Url + framePayload.UrlFragment;
        }

        internal void OnFrameNavigated(FrameNavigatedEventArgs e) => this.FrameNavigated?.Invoke(this, e);

        internal void OnSwapped() => this.FrameSwapped?.Invoke(this, EventArgs.Empty);

        internal void NavigatedWithinDocument(string url)
        {
            this.Url = url;
            this.FrameNavigatedWithinDocument?.Invoke(this, EventArgs.Empty);
        }

        internal void Detach()
        {
            this.Detached = true;
            this.MainWorld.Detach();
            this.PuppeteerWorld.Detach();
            this.FrameDetached?.Invoke(this, EventArgs.Empty);
        }

        internal void OnFrameSwappedByActivation()
            => this.FrameSwappedByActivation?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Gets the prompts manager for the current client.
        /// </summary>
        /// <returns>The <see cref="DeviceRequestPromptManager"/>.</returns>
        protected internal abstract DeviceRequestPromptManager GetDeviceRequestPromptManager();

        private Task<ElementHandle> GetDocumentAsync()
        {
            if (this.documentTask != null)
            {
                return this.documentTask;
            }

            async Task<ElementHandle> EvaluateDocumentInContext()
            {
                var document = await this.IsolatedRealm.EvaluateFunctionHandleAsync("() => document").ConfigureAwait(false);

                if (document is not ElementHandle)
                {
                    throw new PuppeteerException("Document is null");
                }

                return await this.MainRealm.TransferHandleAsync(document).ConfigureAwait(false) as ElementHandle;
            }

            this.documentTask = EvaluateDocumentInContext();

            return this.documentTask;
        }
    }
}
