// <copyright file="DisposableTasksStack.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// This is a DisposableStack in puppeteer. We don't need to dispose objects manually.
/// But this is used to run actions when this object is disposed.
/// </summary>
internal class DisposableTasksStack : IAsyncDisposable, IDisposable
{
    private readonly List<Func<Task>> tasks = [];

    public bool IsDisposed { get; private set; }

    public void Defer(Func<Task> task)
    {
        this.tasks.Add(task);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.IsDisposed)
        {
            return;
        }

        this.IsDisposed = true;
        this.tasks.Reverse();

        foreach (var task in this.tasks)
        {
            await task().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _ = this.DisposeAsync();
    }
}
