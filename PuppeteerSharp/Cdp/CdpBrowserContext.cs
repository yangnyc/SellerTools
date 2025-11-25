// <copyright file="CdpBrowserContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;

/// <inheritdoc />
public class CdpBrowserContext : BrowserContext
{
    private readonly Connection connection;
    private readonly CdpBrowser browser;

    internal CdpBrowserContext(Connection connection, CdpBrowser browser, string contextId)
    {
        this.connection = connection;
        this.Browser = browser;
        this.browser = browser;
        this.Id = contextId;
    }

    /// <inheritdoc/>
    public override ITarget[] Targets() => Array.FindAll(this.Browser.Targets(), target => target.BrowserContext == this);

    /// <inheritdoc/>
    public override async Task<IPage[]> PagesAsync()
        => (await Task.WhenAll(
                this.Targets()
                    .Where(t =>
                        t.Type == TargetType.Page ||
                        (t.Type == TargetType.Other && this.Browser.IsPageTargetFunc(t as Target)))
                    .Select(t => t.PageAsync())).ConfigureAwait(false))
            .Where(p => p != null).ToArray();

    /// <inheritdoc/>
    public override Task<IPage> NewPageAsync() => this.browser.CreatePageInContextAsync(this.Id);

    /// <inheritdoc/>
    public override Task CloseAsync()
    {
        if (this.Id == null)
        {
            throw new PuppeteerException("Non-incognito profiles cannot be closed!");
        }

        return this.browser.DisposeContextAsync(this.Id);
    }

    /// <inheritdoc/>
    public override Task OverridePermissionsAsync(string origin, IEnumerable<OverridePermission> permissions)
        => this.connection.SendAsync("Browser.grantPermissions", new BrowserGrantPermissionsRequest
        {
            Origin = origin,
            BrowserContextId = this.Id,
            Permissions = permissions.ToArray(),
        });

    /// <inheritdoc/>
    public override Task ClearPermissionOverridesAsync()
        => this.connection.SendAsync("Browser.resetPermissions", new BrowserResetPermissionsRequest
        {
            BrowserContextId = this.Id,
        });
}
