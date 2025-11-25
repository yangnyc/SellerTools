// <copyright file="DisposableActionsStack.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System;
using System.Collections.Generic;

/// <summary>
/// This is a DisposableStack in puppeteer. We don't need to dispose objects manually.
/// But this is used to run actions when this object is disposed.
/// </summary>
internal class DisposableActionsStack : IDisposable
{
    private readonly List<Action> actions = [];

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        this.IsDisposed = true;
        this.actions.Reverse();

        foreach (var action in this.actions)
        {
            action();
        }
    }

    public void Defer(Action action)
    {
        this.actions.Add(action);
    }
}
