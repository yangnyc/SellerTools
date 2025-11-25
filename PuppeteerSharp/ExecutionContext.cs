// <copyright file="ExecutionContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using PuppeteerSharp.Cdp;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;

    /// <inheritdoc cref="IExecutionContext"/>
    public sealed partial class ExecutionContext : IExecutionContext, IDisposable, IAsyncDisposable
    {
        internal const string EvaluationScriptUrl = "__puppeteer_evaluation_script__";
        private const string EvaluationScriptSuffix = $"//# sourceURL={EvaluationScriptUrl}";

#if NETSTANDARD2_0
        private static readonly Regex _sourceUrlRegex =
            new(@"^[\040\t]*\/\/[@#] sourceURL=\s*\S*?\s*$", RegexOptions.Multiline);
#endif

        private readonly TaskQueue puppeteerUtilQueue = new();
        private IJSHandle puppeteerUtil;

        internal ExecutionContext(
            CDPSession client,
            ContextPayload contextPayload,
            IsolatedWorld world)
        {
            this.Client = client;
            this.ContextId = contextPayload.Id;
            this.ContextName = contextPayload.Name;
            this.World = world;
        }

        /// <inheritdoc/>
        IFrame IExecutionContext.Frame => this.World?.Frame;

        internal int ContextId { get; }

        internal string ContextName { get; }

        internal CDPSession Client { get; }

        internal IsolatedWorld World { get; }

        private Frame Frame => this.World?.Frame;

        /// <inheritdoc/>
        public Task<JsonElement?> EvaluateExpressionAsync(string script)
            => this.EvaluateExpressionAsync<JsonElement?>(script);

        /// <inheritdoc/>
        public Task<T> EvaluateExpressionAsync<T>(string script)
            => this.RemoteObjectTaskToObject<T>(this.EvaluateExpressionInternalAsync(true, script));

        /// <inheritdoc/>
        public async Task<IJSHandle> EvaluateExpressionHandleAsync(string script)
            => this.CreateJSHandle(await this.EvaluateExpressionInternalAsync(false, script).ConfigureAwait(false));

        /// <inheritdoc/>
        public async Task<IJSHandle> EvaluateFunctionHandleAsync(string script, params object[] args)
            => this.CreateJSHandle(await this.EvaluateFunctionInternalAsync(false, script, args).ConfigureAwait(false));

        /// <inheritdoc/>
        public Task<JsonElement?> EvaluateFunctionAsync(string script, params object[] args)
            => this.EvaluateFunctionAsync<JsonElement?>(script, args);

        /// <inheritdoc/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
            => this.RemoteObjectTaskToObject<T>(this.EvaluateFunctionInternalAsync(true, script, args));

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (this.puppeteerUtilQueue != null)
            {
                await this.puppeteerUtilQueue.DisposeAsync().ConfigureAwait(false);
            }

            if (this.puppeteerUtil != null)
            {
                await this.puppeteerUtil.DisposeAsync().ConfigureAwait(false);
            }

            if (this.World != null)
            {
                await this.World.DisposeAsync().ConfigureAwait(false);
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal async Task<IJSHandle> GetPuppeteerUtilAsync()
        {
            await this.puppeteerUtilQueue.Enqueue(async () =>
            {
                if (this.puppeteerUtil == null)
                {
                    await this.Client.Connection.ScriptInjector.InjectAsync(
                        async (script) =>
                        {
                            if (this.puppeteerUtil != null)
                            {
                                await this.puppeteerUtil.DisposeAsync().ConfigureAwait(false);
                            }

                            await this.InstallGlobalBindingAsync(new Binding(
                                "__ariaQuerySelector",
                                (Func<IElementHandle, string, Task<IElementHandle>>)this.Client.Connection.CustomQuerySelectorRegistry.InternalQueryHandlers["aria"].QueryOneAsync))
                                .ConfigureAwait(false);
                            this.puppeteerUtil = await this.EvaluateExpressionHandleAsync(script).ConfigureAwait(false);
                        },
                        this.puppeteerUtil == null).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
            return this.puppeteerUtil;
        }

        internal IJSHandle CreateJSHandle(RemoteObject remoteObject)
            => remoteObject.Subtype == RemoteObjectSubtype.Node && this.Frame != null
                ? new CdpElementHandle(this.World, remoteObject)
                : new CdpJSHandle(this.World, remoteObject);

#if NET8_0_OR_GREATER
        [GeneratedRegex(@"^[\040\t]*\/\/[@#] sourceURL=\s*\S*?\s*$", RegexOptions.Multiline)]
        private static partial Regex GetSourceUrlRegex();
#else
        private static Regex GetSourceUrlRegex() => _sourceUrlRegex;
#endif

        private static string GetExceptionMessage(EvaluateExceptionResponseDetails exceptionDetails)
        {
            if (exceptionDetails.Exception != null)
            {
                return exceptionDetails.Exception.Description ?? exceptionDetails.Exception.Value;
            }

            var message = exceptionDetails.Text;
            if (exceptionDetails.StackTrace != null)
            {
                foreach (var callframe in exceptionDetails.StackTrace.CallFrames)
                {
                    var location = $"{callframe.Url}:{callframe.LineNumber}:{callframe.ColumnNumber}";
                    var functionName = string.IsNullOrEmpty(callframe.FunctionName) ? "<anonymous>" : callframe.FunctionName;
                    message += $"\n at ${functionName} (${location})";
                }
            }

            return message;
        }

        private async Task InstallGlobalBindingAsync(Binding binding)
        {
            try
            {
                if (this.World != null)
                {
                    this.World.Bindings.TryAdd(binding.Name, binding);
                    await this.World.AddBindingToContextAsync(this, binding.Name).ConfigureAwait(false);
                }
            }
            catch
            {
                // If the binding cannot be added, then either the browser doesn't support
                // bindings (e.g. Firefox) or the context is broken. Either breakage is
                // okay, so we ignore the error.
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        private void Dispose(bool disposing) => _ = this.DisposeAsync();

        private async Task<T> RemoteObjectTaskToObject<T>(Task<RemoteObject> remote)
        {
            var response = await remote.ConfigureAwait(false);
            return response == null ? default : (T)RemoteObjectHelper.ValueFromRemoteObject<T>(response);
        }

        private Task<RemoteObject> EvaluateExpressionInternalAsync(bool returnByValue, string script)
            => this.ExecuteEvaluationAsync("Runtime.evaluate", new Dictionary<string, object>
            {
                ["expression"] = GetSourceUrlRegex().IsMatch(script) ? script : $"{script}\n{EvaluationScriptSuffix}",
                ["contextId"] = this.ContextId,
                ["returnByValue"] = returnByValue,
                ["awaitPromise"] = true,
                ["userGesture"] = true,
            });

        private async Task<RemoteObject> EvaluateFunctionInternalAsync(bool returnByValue, string script, params object[] args)
            => await this.ExecuteEvaluationAsync("Runtime.callFunctionOn", new RuntimeCallFunctionOnRequest
            {
                FunctionDeclaration = $"{script}\n{EvaluationScriptSuffix}\n",
                ExecutionContextId = this.ContextId,
                Arguments = await Task.WhenAll(args.Select(this.FormatArgumentAsync).ToArray()).ConfigureAwait(false),
                ReturnByValue = returnByValue,
                AwaitPromise = true,
                UserGesture = true,
            }).ConfigureAwait(false);

        private async Task<RemoteObject> ExecuteEvaluationAsync(string method, object args)
        {
            try
            {
                var response = await this.Client.SendAsync<EvaluateHandleResponse>(method, args).ConfigureAwait(false);

                if (response.ExceptionDetails != null)
                {
                    throw new EvaluationFailedException("Evaluation failed: " +
                        GetExceptionMessage(response.ExceptionDetails));
                }

                return response.Result;
            }
            catch (MessageException ex)
            {
                if (ex.Message.Contains("Object reference chain is too long") ||
                    ex.Message.Contains("Object couldn't be returned by value"))
                {
                    return default;
                }

                throw new EvaluationFailedException(ex.Message, ex);
            }
        }

        private async Task<object> FormatArgumentAsync(object arg)
        {
            if (arg is TaskCompletionSource<object> tcs)
            {
                arg = await tcs.Task.ConfigureAwait(false);
            }

            if (arg is LazyArg lazyArg)
            {
                arg = await lazyArg(this).ConfigureAwait(false);
            }

            switch (arg)
            {
                case BigInteger big:
                    return new RuntimeCallFunctionOnRequestArgumentValue()
                    {
                        UnserializableValue = $"{big}n",
                    };

                case int integer when integer == -0:
                    return new RuntimeCallFunctionOnRequestArgumentValue()
                    {
                        UnserializableValue = "-0",
                    };

                case double d:
                    if (double.IsPositiveInfinity(d))
                    {
                        return new RuntimeCallFunctionOnRequestArgumentValue()
                        {
                            UnserializableValue = "Infinity",
                        };
                    }

                    if (double.IsNegativeInfinity(d))
                    {
                        return new RuntimeCallFunctionOnRequestArgumentValue()
                        {
                            UnserializableValue = "-Infinity",
                        };
                    }

                    if (double.IsNaN(d))
                    {
                        return new { unserializableValue = "NaN" };
                    }

                    break;

                case IJSHandle objectHandle:
                    return objectHandle.FormatArgument(this);
            }

            return new RuntimeCallFunctionOnRequestArgument
            {
                Value = arg,
            };
        }
    }
}
