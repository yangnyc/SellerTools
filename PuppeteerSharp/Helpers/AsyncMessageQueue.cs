// <copyright file="AsyncMessageQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using PuppeteerSharp.Cdp.Messaging;

    /// <summary>
    /// Provides an async queue for responses for <see cref="CDPSession.SendAsync"/>, so that responses can be handled
    /// async without risk callers causing a deadlock.
    /// </summary>
    internal sealed class AsyncMessageQueue : IDisposable
    {
        private readonly List<MessageTask> pendingTasks = new();
        private readonly bool enqueueAsyncMessages;
        private readonly ILogger logger;
        private bool disposed;

        public AsyncMessageQueue(bool enqueueAsyncMessages, ILogger logger = null)
        {
            this.enqueueAsyncMessages = enqueueAsyncMessages;
            this.logger = logger ?? NullLogger.Instance;
        }

        public void Enqueue(MessageTask callback, ConnectionResponse obj)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (!this.enqueueAsyncMessages)
            {
                HandleAsyncMessage(callback, obj);
                return;
            }

            // Keep a ref to this task until it completes. If it can't finish by the time we dispose this queue,
            // then we'll find it and cancel it.
            lock (this.pendingTasks)
            {
                this.pendingTasks.Add(callback);
            }

            var task = Task.Run(() => HandleAsyncMessage(callback, obj));

            // Unhandled error handler
            task.ContinueWith(
                t =>
                {
                    this.logger.LogError(t.Exception, "Failed to complete async handling of SendAsync for {Method}", callback.Method);
                    callback.TaskWrapper.TrySetException(t.Exception!); // t.Exception is available since this runs only on faulted
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);

            // Always remove from the queue when done, regardless of outcome.
            task.ContinueWith(
                _ =>
                {
                    lock (this.pendingTasks)
                    {
                        this.pendingTasks.Remove(callback);
                    }
                },
                TaskScheduler.Default);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            // Ensure all tasks are finished since we're disposing now. Any pending tasks will be canceled.
            MessageTask[] pendingTasks;
            lock (this.pendingTasks)
            {
                pendingTasks = this.pendingTasks.ToArray();
                this.pendingTasks.Clear();
            }

            foreach (var pendingTask in pendingTasks)
            {
                pendingTask.TaskWrapper.TrySetCanceled();
            }

            this.disposed = true;
        }

        private static void HandleAsyncMessage(MessageTask callback, ConnectionResponse obj)
        {
            if (obj.Error != null)
            {
                callback.TaskWrapper.TrySetException(new MessageException(callback, obj.Error));
            }
            else
            {
                callback.TaskWrapper.TrySetResult(obj.Result);
            }
        }
    }
}
