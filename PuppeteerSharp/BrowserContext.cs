// <copyright file="BrowserContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <inheritdoc/>
    public abstract class BrowserContext : IBrowserContext
    {
        /// <inheritdoc/>
        public event EventHandler<TargetChangedArgs> TargetChanged;

        /// <inheritdoc/>
        public event EventHandler<TargetChangedArgs> TargetCreated;

        /// <inheritdoc/>
        public event EventHandler<TargetChangedArgs> TargetDestroyed;

        /// <inheritdoc/>
        public string Id { get; protected init; }

        /// <inheritdoc/>
        public bool IsClosed => !this.Browser.BrowserContexts().Contains(this);

        /// <inheritdoc cref="Browser"/>
        public Browser Browser { get; protected init; }

        /// <inheritdoc/>
        IBrowser IBrowserContext.Browser => this.Browser;

        /// <inheritdoc/>
        public abstract ITarget[] Targets();

        /// <inheritdoc/>
        public Task<ITarget> WaitForTargetAsync(Func<ITarget, bool> predicate, WaitForOptions options = null)
            => this.Browser.WaitForTargetAsync((target) => target.BrowserContext == this && predicate(target), options);

        /// <inheritdoc />
        public abstract Task<IPage[]> PagesAsync();

        /// <inheritdoc/>
        public abstract Task<IPage> NewPageAsync();

        /// <inheritdoc/>
        public abstract Task CloseAsync();

        /// <inheritdoc/>
        public abstract Task OverridePermissionsAsync(string origin, IEnumerable<OverridePermission> permissions);

        /// <inheritdoc/>
        public abstract Task ClearPermissionOverridesAsync();

        internal void OnTargetCreated(Browser browser, TargetChangedArgs args) => this.TargetCreated?.Invoke(browser, args);

        internal void OnTargetDestroyed(Browser browser, TargetChangedArgs args) => this.TargetDestroyed?.Invoke(browser, args);

        internal void OnTargetChanged(Browser browser, TargetChangedArgs args) => this.TargetChanged?.Invoke(browser, args);
    }
}
