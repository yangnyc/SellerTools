// <copyright file="CdpJSHandle.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;

/// <inheritdoc/>
public class CdpJSHandle : JSHandle
{
    internal CdpJSHandle(IsolatedWorld world, RemoteObject remoteObject)
    {
        this.Realm = world;
        this.RemoteObject = remoteObject;
        this.Logger = this.Client.Connection.LoggerFactory.CreateLogger(this.GetType());
    }

    /// <inheritdoc/>
    public override RemoteObject RemoteObject { get; }

    internal override IsolatedWorld Realm { get; }

    /// <summary>
    /// Gets logger.
    /// </summary>
    private ILogger Logger { get; }

    private CDPSession Client => this.Realm.Environment.Client;

    /// <inheritdoc/>
    public override async Task<T> JsonValueAsync<T>()
    {
        var objectId = this.RemoteObject.ObjectId;

        if (objectId == null)
        {
            return (T)RemoteObjectHelper.ValueFromRemoteObject<T>(this.RemoteObject);
        }

        var value = await this.EvaluateFunctionAsync<T>("object => object").ConfigureAwait(false);

        return value ?? throw new PuppeteerException("Could not serialize referenced object");
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (this.Disposed)
        {
            return;
        }

        this.Disposed = true;

        if (this.DisposeAction != null)
        {
            await this.DisposeAction().ConfigureAwait(false);
        }

        await RemoteObjectHelper.ReleaseObjectAsync(this.Client, this.RemoteObject, this.Logger).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.RemoteObject.ObjectId == null)
        {
            return "JSHandle:" + RemoteObjectHelper.ValueFromRemoteObject<object>(this.RemoteObject, true);
        }

        var type = this.RemoteObject.Subtype != RemoteObjectSubtype.Other
            ? this.RemoteObject.Subtype.ToString()
            : this.RemoteObject.Type.ToString();
        return "JSHandle@" + type.ToLower(System.Globalization.CultureInfo.CurrentCulture);
    }
}
