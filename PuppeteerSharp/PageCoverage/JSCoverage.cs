// <copyright file="JSCoverage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers.Json;

    internal class JSCoverage
    {
        private readonly ConcurrentDictionary<string, string> scriptURLs = new();
        private readonly ConcurrentDictionary<string, string> scriptSources = new();
        private readonly ILogger logger;

        private CDPSession client;
        private bool enabled;
        private bool resetOnNavigation;
        private bool reportAnonymousScripts;
        private bool includeRawScriptCoverage;
        private bool useBlockCoverage;

        public JSCoverage(CDPSession client)
        {
            this.client = client;
            this.enabled = false;
            this.logger = this.client.Connection.LoggerFactory.CreateLogger<JSCoverage>();

            this.resetOnNavigation = false;
        }

        internal void UpdateClient(CDPSession client) => this.client = client;

        internal Task StartAsync(CoverageStartOptions options)
        {
            if (this.enabled)
            {
                throw new InvalidOperationException("JSCoverage is already enabled");
            }

            this.resetOnNavigation = options.ResetOnNavigation;
            this.reportAnonymousScripts = options.ReportAnonymousScripts;
            this.includeRawScriptCoverage = options.IncludeRawScriptCoverage;
            this.useBlockCoverage = options.UseBlockCoverage;
            this.enabled = true;
            this.scriptURLs.Clear();
            this.scriptSources.Clear();

            this.client.MessageReceived += this.Client_MessageReceived;

            return Task.WhenAll(
                this.client.SendAsync("Profiler.enable"),
                this.client.SendAsync("Profiler.startPreciseCoverage", new ProfilerStartPreciseCoverageRequest
                {
                    CallCount = this.includeRawScriptCoverage,
                    Detailed = this.useBlockCoverage,
                }),
                this.client.SendAsync("Debugger.enable"),
                this.client.SendAsync("Debugger.setSkipAllPauses", new DebuggerSetSkipAllPausesRequest { Skip = true }));
        }

        internal async Task<JSCoverageEntry[]> StopAsync()
        {
            if (!this.enabled)
            {
                throw new InvalidOperationException("JSCoverage is not enabled");
            }

            this.enabled = false;

            var profileResponseTask = this.client.SendAsync<ProfilerTakePreciseCoverageResponse>("Profiler.takePreciseCoverage");
            await Task.WhenAll(
               profileResponseTask,
               this.client.SendAsync("Profiler.stopPreciseCoverage"),
               this.client.SendAsync("Profiler.disable"),
               this.client.SendAsync("Debugger.disable")).ConfigureAwait(false);
            this.client.MessageReceived -= this.Client_MessageReceived;

            var coverage = new List<JSCoverageEntry>();
            foreach (var entry in profileResponseTask.Result.Result)
            {
                this.scriptURLs.TryGetValue(entry.ScriptId, out var url);
                if (string.IsNullOrEmpty(url) && this.reportAnonymousScripts)
                {
                    url = "debugger://VM" + entry.ScriptId;
                }

                if (string.IsNullOrEmpty(url) ||
                    !this.scriptSources.TryGetValue(entry.ScriptId, out var text))
                {
                    continue;
                }

                var flattenRanges = entry.Functions.SelectMany(f => f.Ranges);
                var ranges = Coverage.ConvertToDisjointRanges(flattenRanges);
                coverage.Add(new JSCoverageEntry
                {
                    Url = url,
                    Ranges = ranges,
                    Text = text,
                    RawScriptCoverage = this.includeRawScriptCoverage ? entry : null,
                });
            }

            return [.. coverage];
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "Debugger.scriptParsed":
                        await this.OnScriptParsedAsync(e.MessageData.ToObject<DebuggerScriptParsedResponse>()).ConfigureAwait(false);
                        break;
                    case "Runtime.executionContextsCleared":
                        this.OnExecutionContextsCleared();
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"JSCoverage failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                this.client.Close(message);
            }
        }

        private async Task OnScriptParsedAsync(DebuggerScriptParsedResponse scriptParseResponse)
        {
            if (scriptParseResponse.Url == ExecutionContext.EvaluationScriptUrl ||
                (string.IsNullOrEmpty(scriptParseResponse.Url) && !this.reportAnonymousScripts))
            {
                return;
            }

            try
            {
                var response = await this.client.SendAsync<DebuggerGetScriptSourceResponse>("Debugger.getScriptSource", new DebuggerGetScriptSourceRequest
                {
                    ScriptId = scriptParseResponse.ScriptId,
                }).ConfigureAwait(false);
                this.scriptURLs.TryAdd(scriptParseResponse.ScriptId, scriptParseResponse.Url);
                this.scriptSources.TryAdd(scriptParseResponse.ScriptId, response.ScriptSource);
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

            this.scriptURLs.Clear();
            this.scriptSources.Clear();
        }
    }
}
