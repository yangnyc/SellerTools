// <copyright file="TaskManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Linq;
    using PuppeteerSharp.Helpers;

    internal class TaskManager
    {
        private ConcurrentSet<WaitTask> WaitTasks { get; } = new();

        internal void Add(WaitTask waitTask) => this.WaitTasks.Add(waitTask);

        internal void Delete(WaitTask waitTask) => this.WaitTasks.Remove(waitTask);

        internal void RerunAll()
        {
            foreach (var waitTask in this.WaitTasks)
            {
                _ = waitTask.RerunAsync();
            }
        }

        internal void TerminateAll(Exception exception)
        {
            while (!this.WaitTasks.IsEmpty)
            {
                _ = this.WaitTasks.First().TerminateAsync(exception);
            }
        }
    }
}
