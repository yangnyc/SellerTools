// <copyright file="DeferredTaskQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An async queue that accepts a task but defers its execution to be handled by a consumer queue.
    /// </summary>
    internal class DeferredTaskQueue
    {
        private readonly List<Task> pendingTasks = new();

        public async Task Enqueue(Func<Task> taskGenerator)
        {
            var task = taskGenerator();
            if (task.IsCompleted)
            {
                // Don't need to do anything.
                return;
            }

            try
            {
                lock (this.pendingTasks)
                {
                    this.pendingTasks.Add(task);
                }

                await task.ConfigureAwait(false);
            }
            finally
            {
                lock (this.pendingTasks)
                {
                    this.pendingTasks.Remove(task);
                }
            }
        }

        public async Task DrainAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Task task;
                lock (this.pendingTasks)
                {
                    if (this.pendingTasks.Count == 0)
                    {
                        break;
                    }

                    task = this.pendingTasks[0];
                }

                try
                {
                    await task.ConfigureAwait(false);
                }
                finally
                {
                    lock (this.pendingTasks)
                    {
                        this.pendingTasks.Remove(task);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
