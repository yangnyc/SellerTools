// <copyright file="IsolatedWorld.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// A LazyArg is an evaluation argument that will be resolved when the CDP call is built.
    /// </summary>
    /// <param name="context">Execution context.</param>
    /// <returns>Resolved argument.</returns>
    public delegate Task<object> LazyArg(ExecutionContext context);

    internal class IsolatedWorld : Realm, IDisposable, IAsyncDisposable
    {
        private readonly ILogger logger;
        private readonly List<string> contextBindings = new();
        private readonly TaskQueue bindingQueue = new();
        private bool detached;
        private TaskCompletionSource<ExecutionContext> contextResolveTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private ExecutionContext context;

        public IsolatedWorld(
            Frame frame,
            WebWorker worker,
            TimeoutSettings timeoutSettings,
            bool isMainWorld)
            : base(timeoutSettings)
        {
            this.Frame = frame;
            this.Worker = worker;
            this.IsMainWorld = isMainWorld;
            this.logger = this.Client.Connection.LoggerFactory.CreateLogger<IsolatedWorld>();

            this.detached = false;
            this.FrameUpdated();
        }

        /// <summary>
        /// Gets a value indicating whether this property is not upstream. It's helpful for debugging.
        /// </summary>
        internal bool IsMainWorld { get; }

        internal Frame Frame { get; }

        internal CDPSession Client => this.Frame?.Client ?? this.Worker?.Client;

        internal bool HasContext => this.contextResolveTaskWrapper?.Task.IsCompleted == true;

        internal ConcurrentDictionary<string, Binding> Bindings { get; } = new();

        internal override IEnvironment Environment => (IEnvironment)this.Frame ?? this.Worker;

        private WebWorker Worker { get; }

        public void Dispose()
        {
            this.bindingQueue.Dispose();
            this.context?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await this.bindingQueue.DisposeAsync().ConfigureAwait(false);
            if (this.context != null)
            {
                await this.context.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void FrameUpdated() => this.Client.MessageReceived += this.Client_MessageReceived;

        internal async Task AddBindingToContextAsync(ExecutionContext context, string name)
        {
            // Previous operation added the binding so we are done.
            if (this.contextBindings.Contains(name))
            {
                return;
            }

            await this.bindingQueue.Enqueue(async () =>
            {
                var expression = BindingUtils.PageBindingInitString("internal", name);
                try
                {
                    // TODO: In theory, it would be enough to call this just once
                    await context.Client.SendAsync(
                        "Runtime.addBinding",
                        new RuntimeAddBindingRequest
                        {
                            Name = name,
                            ExecutionContextName = !string.IsNullOrEmpty(context.ContextName) ? context.ContextName : null,
                            ExecutionContextId = string.IsNullOrEmpty(context.ContextName) ? context.ContextId : null,
                        }).ConfigureAwait(false);

                    await context.EvaluateExpressionAsync(expression).ConfigureAwait(false);
                    this.contextBindings.Add(name);
                }
                catch (Exception ex)
                {
                    var ctxDestroyed = ex.Message.Contains("Execution context was destroyed");
                    var ctxNotFound = ex.Message.Contains("Cannot find context with specified id");
                    if (ctxDestroyed || ctxNotFound)
                    {
                        return;
                    }

                    this.logger.LogError(ex.ToString());
                }
            }).ConfigureAwait(false);
        }

        internal override async Task<IElementHandle> AdoptBackendNodeAsync(object backendNodeId)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            var obj = await this.Client.SendAsync<DomResolveNodeResponse>("DOM.resolveNode", new DomResolveNodeRequest
            {
                BackendNodeId = backendNodeId,
                ExecutionContextId = context.ContextId,
            }).ConfigureAwait(false);

            return context.CreateJSHandle(obj.Object) as IElementHandle;
        }

        internal override async Task<IJSHandle> TransferHandleAsync(IJSHandle handle)
        {
            if ((handle as JSHandle)?.Realm == this)
            {
                return handle;
            }

            if (handle.RemoteObject.ObjectId == null)
            {
                return handle;
            }

            var info = await this.Client.SendAsync<DomDescribeNodeResponse>(
                "DOM.describeNode",
                new DomDescribeNodeRequest
                {
                    ObjectId = handle.RemoteObject.ObjectId,
                }).ConfigureAwait(false);

            var newHandle = await this.AdoptBackendNodeAsync(info.Node.BackendNodeId).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
            return newHandle;
        }

        internal override async Task<IJSHandle> AdoptHandleAsync(IJSHandle handle)
        {
            if ((handle as JSHandle)?.Realm == this)
            {
                return handle;
            }

            var nodeInfo = await this.Client.SendAsync<DomDescribeNodeResponse>("DOM.describeNode", new DomDescribeNodeRequest
            {
                ObjectId = ((JSHandle)handle).RemoteObject.ObjectId,
            }).ConfigureAwait(false);
            return await this.AdoptBackendNodeAsync(nodeInfo.Node.BackendNodeId).ConfigureAwait(false);
        }

        internal void Detach()
        {
            this.detached = true;
            this.Client.MessageReceived -= this.Client_MessageReceived;
            this.TaskManager.TerminateAll(new PuppeteerException("waitForFunction failed: frame got detached."));
        }

        internal Task<ExecutionContext> GetExecutionContextAsync()
        {
            if (this.detached)
            {
                throw new PuppeteerException($"Execution Context is not available in detached frame \"{this.Frame.Url}\" (are you trying to evaluate?)");
            }

            return this.contextResolveTaskWrapper.Task;
        }

        internal override async Task<IJSHandle> EvaluateExpressionHandleAsync(string script)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionHandleAsync(script).ConfigureAwait(false);
        }

        internal override async Task<IJSHandle> EvaluateFunctionHandleAsync(string script, params object[] args)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionHandleAsync(script, args).ConfigureAwait(false);
        }

        internal override async Task<T> EvaluateExpressionAsync<T>(string script)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionAsync<T>(script).ConfigureAwait(false);
        }

        internal override async Task<JsonElement?> EvaluateExpressionAsync(string script)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionAsync(script).ConfigureAwait(false);
        }

        internal override async Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionAsync<T>(script, args).ConfigureAwait(false);
        }

        internal override async Task<JsonElement?> EvaluateFunctionAsync(string script, params object[] args)
        {
            var context = await this.GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionAsync(script, args).ConfigureAwait(false);
        }

        internal void ClearContext()
        {
            this.contextResolveTaskWrapper.TrySetException(new PuppeteerException("Execution Context was destroyed"));
            this.contextResolveTaskWrapper = new TaskCompletionSource<ExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.context?.Dispose();
            this.context = null;
            this.Frame?.ClearDocumentHandle();
        }

        internal void SetNewContext(
            CDPSession client,
            ContextPayload contextPayload,
            IsolatedWorld world)
        {
            this.context = new ExecutionContext(
                client,
                contextPayload,
                world);

            this.SetContext(this.context);
        }

        internal void SetContext(ExecutionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.contextBindings.Clear();
            this.contextResolveTaskWrapper.TrySetResult(context);
            this.TaskManager.RerunAll();
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "Runtime.bindingCalled":
                        await this.OnBindingCalledAsync(e.MessageData.ToObject<BindingCalledResponse>()).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"IsolatedWorld failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                this.Client.Close(message);
            }
        }

        private async Task OnBindingCalledAsync(BindingCalledResponse e)
        {
            var payload = e.BindingPayload;

            if (payload.Type != "internal")
            {
                return;
            }

            if (!this.contextBindings.Contains(payload.Name))
            {
                return;
            }

            try
            {
                var context = await this.GetExecutionContextAsync().ConfigureAwait(false);

                if (e.ExecutionContextId != context.ContextId)
                {
                    return;
                }

                if (this.Bindings.TryGetValue(payload.Name, out var binding))
                {
                    await binding.RunAsync(context, payload.Seq, payload.Args.Cast<object>().ToArray(), payload.IsTrivial).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Protocol error"))
                {
                    return;
                }

                this.logger.LogError(ex.ToString());
            }
        }
    }
}
