// <copyright file="WaitTask.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class WaitTask : IDisposable
    {
        private readonly Realm realm;
        private readonly string fn;
        private readonly WaitForFunctionPollingOption? polling;
        private readonly int? pollingInterval;
        private readonly object[] args;
        private readonly Task timeoutTimer;
        private readonly IElementHandle root;
        private readonly CancellationTokenSource cts = new();
        private readonly TaskCompletionSource<IJSHandle> result = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool isDisposed;
        private IJSHandle poller;
        private bool terminated;

        internal WaitTask(
            Realm realm,
            string fn,
            bool isExpression,
            WaitForFunctionPollingOption polling,
            int? pollingInterval,
            int timeout,
            IElementHandle root,
            object[] args = null)
        {
            if (string.IsNullOrEmpty(fn))
            {
                throw new ArgumentNullException(nameof(fn));
            }

            if (pollingInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Cannot poll with non-positive interval");
            }

            this.realm = realm;
            this.fn = isExpression ? $"() => {{return ({fn});}}" : fn;
            this.pollingInterval = pollingInterval;
            this.polling = this.pollingInterval.HasValue ? null : polling;
            this.args = args ?? [];
            this.root = root;

            this.realm.TaskManager.Add(this);

            if (timeout > 0)
            {
                this.timeoutTimer = System.Threading.Tasks.Task.Delay(timeout, this.cts.Token)
                    .ContinueWith(
                        _ => this.TerminateAsync(new WaitTaskTimeoutException(timeout)),
                        TaskScheduler.Default);
            }

            _ = this.RerunAsync();
        }

        internal Task<IJSHandle> Task => this.result.Task;

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            if (this.timeoutTimer is { Status: TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled } timeoutTimer)
            {
                timeoutTimer.Dispose();
            }

            this.cts.Dispose();

            this.isDisposed = true;
        }

        internal async Task RerunAsync()
        {
            try
            {
                if (this.pollingInterval.HasValue)
                {
                    this.poller = await this.realm.EvaluateFunctionHandleAsync(
                            @"
                            ({IntervalPoller, createFunction}, ms, fn, ...args) => {
                                const fun = createFunction(fn);
                                return new IntervalPoller(() => {
                                    return fun(...args);
                                }, ms);
                            }",
                            [
                                new LazyArg(async context => await context.GetPuppeteerUtilAsync().ConfigureAwait(false)),
                                this.pollingInterval,
                                this.fn,
                                .. this.args,
                            ]).ConfigureAwait(false);
                }
                else if (this.polling == WaitForFunctionPollingOption.Raf)
                {
                    this.poller = await this.realm.EvaluateFunctionHandleAsync(
                            @"
                            ({RAFPoller, createFunction}, fn, ...args) => {
                                const fun = createFunction(fn);
                                return new RAFPoller(() => {
                                    return fun(...args);
                                });
                            }",
                            [
                                new LazyArg(async context => await context.GetPuppeteerUtilAsync().ConfigureAwait(false)),
                                this.fn,
                                ..this.args
                            ]).ConfigureAwait(false);
                }
                else
                {
                    this.poller = await this.realm.EvaluateFunctionHandleAsync(
                            @"
                            ({MutationPoller, createFunction}, root, fn, ...args) => {
                                const fun = createFunction(fn);
                                return new MutationPoller(() => {
                                    return fun(...args);
                                }, root || document);
                            }",
                            [
                                new LazyArg(async context => await context.GetPuppeteerUtilAsync().ConfigureAwait(false)),
                                this.root,
                                this.fn,
                                ..this.args
                            ]).ConfigureAwait(false);
                }

                // Note that FrameWaitForFunctionTests listen for this particular message to orchestrate the test execution
                await this.poller.EvaluateFunctionAsync("poller => poller.start()").ConfigureAwait(false);

                var success = await this.poller.EvaluateFunctionHandleAsync("poller => poller.result()").ConfigureAwait(false);
                this.result.TrySetResult(success);
                await this.TerminateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exception = this.GetBadException(ex);
                if (exception != null)
                {
                    await this.TerminateAsync(exception).ConfigureAwait(false);
                }
            }
        }

        internal async Task TerminateAsync(Exception exception = null)
        {
            // The timeout timer might call this method on cleanup
            if (this.terminated)
            {
                return;
            }

            this.terminated = true;
            this.realm.TaskManager.Delete(this);
            this.Cleanup(); // This matches the clearTimeout upstream

            if (exception != null)
            {
                this.result.TrySetException(exception);
            }

            if (this.poller is { } poller)
            {
                await using (poller.ConfigureAwait(false))
                {
                    try
                    {
                        await poller.EvaluateFunctionAsync(@"async poller => {
                            await poller.stop();
                        }").ConfigureAwait(false);

                        this.poller = null;
                    }
                    catch (Exception)
                    {
                        // swallow error.
                    }
                }
            }
        }

        private Exception GetBadException(Exception exception)
        {
            // When frame is detached the task should have been terminated by the IsolatedWorld.
            // This can fail if we were adding this task while the frame was detached,
            // so we terminate here instead.
            if (exception.Message.Contains("Execution context is not available in detached frame"))
            {
                return new PuppeteerException("Waiting failed: Frame detached", exception);
            }

            // When the page is navigated, the promise is rejected.
            // We will try again in the new execution context.
            if (exception.Message.Contains("Execution context was destroyed"))
            {
                return null;
            }

            // We could have tried to evaluate in a context which was already destroyed.
            if (exception.Message.Contains("Cannot find context with specified id"))
            {
                return null;
            }

            // We don't have this check upstream.
            // We have a situation in our async code where a new navigation could be executed
            // before the WaitForFunction completes its initialization
            // See FrameWaitForSelectorTests.ShouldSurviveCrossProcessNavigation
            if (exception.Message.Contains("JSHandles can be evaluated only in the context they were created!"))
            {
                return null;
            }

            // This is a different message coming from Firefox in the same situation.
            // This is not upstream.
            if (exception.Message.Contains("Could not find object with given id"))
            {
                return null;
            }

            return exception;
        }

        private void Cleanup()
        {
            if (!this.cts.IsCancellationRequested)
            {
                try
                {
                    this.cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore
                }
            }
        }
    }
}
