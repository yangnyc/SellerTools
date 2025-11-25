// <copyright file="TaskQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class TaskQueue : IDisposable, IAsyncDisposable
    {
        private readonly SemaphoreSlim semaphore;
        private readonly AsyncLocal<bool> held = new();
        private int disposed;

        internal TaskQueue() => this.semaphore = new SemaphoreSlim(1);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) != 0)
            {
                return;
            }

            if (!this.held.Value)
            {
                this.semaphore.Wait();
            }

            this.semaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) != 0)
            {
                return;
            }

            if (!this.held.Value)
            {
                await this.semaphore.WaitAsync().ConfigureAwait(false);
            }

            this.semaphore.Dispose();
        }

        internal async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await this.semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                this.held.Value = true;
                return await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                this.TryRelease(this.semaphore);
                this.held.Value = false;
            }
        }

        internal async Task Enqueue(Func<Task> taskGenerator)
        {
            await this.semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                this.held.Value = true;
                await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                this.TryRelease(this.semaphore);
                this.held.Value = false;
            }
        }

        private void TryRelease(SemaphoreSlim semaphore)
        {
            try
            {
                semaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // If semaphore has already been disposed, then Release() will fail
                // but we can safely ignore it
            }
        }
    }
}
