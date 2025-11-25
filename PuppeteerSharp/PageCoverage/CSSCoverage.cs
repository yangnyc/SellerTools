// <copyright file="CSSCoverage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    internal class CSSCoverage
    {
        private readonly ConcurrentDictionary<string, (string Url, string Source)> stylesheets = new();
        private readonly DeferredTaskQueue callbackQueue = new();
        private readonly ILogger logger;

        private CDPSession client;
        private bool enabled;
        private bool resetOnNavigation;

        public CSSCoverage(CDPSession client)
        {
            this.client = client;
            this.enabled = false;
            this.logger = this.client.Connection.LoggerFactory.CreateLogger<CSSCoverage>();
            this.resetOnNavigation = false;
        }

        internal void UpdateClient(CDPSession client) => this.client = client;

        internal Task StartAsync(CoverageStartOptions options)
        {
            if (this.enabled)
            {
                throw new InvalidOperationException("CSSCoverage is already enabled");
            }

            this.resetOnNavigation = options.ResetOnNavigation;
            this.enabled = true;
            this.stylesheets.Clear();

            this.client.MessageReceived += this.Client_MessageReceived;

            return Task.WhenAll(
                this.client.SendAsync("DOM.enable"),
                this.client.SendAsync("CSS.enable"),
                this.client.SendAsync("CSS.startRuleUsageTracking"));
        }

        internal async Task<CoverageEntry[]> StopAsync()
        {
            if (!this.enabled)
            {
                throw new InvalidOperationException("CSSCoverage is not enabled");
            }

            this.enabled = false;

            var trackingResponse = await this.client.SendAsync<CSSStopRuleUsageTrackingResponse>("CSS.stopRuleUsageTracking").ConfigureAwait(false);

            // Wait until we've stopped CSS tracking before stopping listening for messages and finishing up, so that
            // any pending OnStyleSheetAddedAsync tasks can collect the remaining style sheet coverage.
            this.client.MessageReceived -= this.Client_MessageReceived;
            await this.callbackQueue.DrainAsync().ConfigureAwait(false);

            await Task.WhenAll(
                this.client.SendAsync("CSS.disable"),
                this.client.SendAsync("DOM.disable")).ConfigureAwait(false);

            var styleSheetIdToCoverage = new Dictionary<string, List<CoverageRange>>();
            foreach (var entry in trackingResponse.RuleUsage)
            {
                styleSheetIdToCoverage.TryGetValue(entry.StyleSheetId, out var ranges);
                if (ranges == null)
                {
                    ranges = new List<CoverageRange>();
                    styleSheetIdToCoverage[entry.StyleSheetId] = ranges;
                }

                ranges.Add(new CoverageRange
                {
                    StartOffset = entry.StartOffset,
                    EndOffset = entry.EndOffset,
                    Count = entry.Used ? 1 : 0,
                });
            }

            var coverage = new List<CoverageEntry>();
            foreach (var kv in this.stylesheets.ToArray())
            {
                var styleSheetId = kv.Key;
                var url = kv.Value.Url;
                var text = kv.Value.Source;
                styleSheetIdToCoverage.TryGetValue(styleSheetId, out var responseRanges);
                var ranges = Coverage.ConvertToDisjointRanges(responseRanges ?? []);
                coverage.Add(new CoverageEntry
                {
                    Url = url,
                    Ranges = ranges,
                    Text = text,
                });
            }

            return coverage.ToArray();
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "CSS.styleSheetAdded":
                        await this.callbackQueue.Enqueue(()
                            => this.OnStyleSheetAddedAsync(e.MessageData.ToObject<CSSStyleSheetAddedResponse>())).ConfigureAwait(false);
                        break;
                    case "Runtime.executionContextsCleared":
                        this.OnExecutionContextsCleared();
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"CSSCoverage failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                this.client.Close(message);
            }
        }

        private async Task OnStyleSheetAddedAsync(CSSStyleSheetAddedResponse styleSheetAddedResponse)
        {
            if (string.IsNullOrEmpty(styleSheetAddedResponse.Header.SourceURL))
            {
                return;
            }

            try
            {
                var response = await this.client.SendAsync<CssGetStyleSheetTextResponse>("CSS.getStyleSheetText", new CssGetStyleSheetTextRequest
                {
                    StyleSheetId = styleSheetAddedResponse.Header.StyleSheetId,
                }).ConfigureAwait(false);

                this.stylesheets.TryAdd(styleSheetAddedResponse.Header.StyleSheetId, (styleSheetAddedResponse.Header.SourceURL, response.Text));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.ToString());
            }
        }

        private void OnExecutionContextsCleared()
        {
            if (!this.resetOnNavigation)
            {
                return;
            }

            this.stylesheets.Clear();
        }
    }
}
