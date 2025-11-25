// <copyright file="JSHandle.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using PuppeteerSharp.Cdp.Messaging;

    /// <inheritdoc/>
    public abstract class JSHandle : IJSHandle
    {
        internal JSHandle()
        {
        }

        /// <inheritdoc/>
        public bool Disposed { get; protected set; }

        /// <inheritdoc/>
        public abstract RemoteObject RemoteObject { get; }

        internal Func<Task> DisposeAction { get; set; }

        internal abstract IsolatedWorld Realm { get; }

        internal Frame Frame => this.Realm.Environment as Frame;

        internal string Id => this.RemoteObject.ObjectId;

        /// <inheritdoc/>
        public virtual Task<IJSHandle> GetPropertyAsync(string propertyName)
            => this.EvaluateFunctionHandleAsync(
                @"(object, propertyName) => {
                        return object[propertyName];
                    }",
                propertyName);

        /// <inheritdoc/>
        public virtual async Task<Dictionary<string, IJSHandle>> GetPropertiesAsync()
        {
            var propertyNames = await this.EvaluateFunctionAsync<string[]>(@"object => {
                    const enumerableProperties = [];
                    const descriptors = Object.getOwnPropertyDescriptors(object);
                    for (const propertyName in descriptors) {
                        if (descriptors[propertyName]?.enumerable)
                        {
                            enumerableProperties.push(propertyName);
                        }
                    }
                    return enumerableProperties;
                }").ConfigureAwait(false);

            var dic = new Dictionary<string, IJSHandle>();

            foreach (var key in propertyNames)
            {
                var handleItem = await this.GetPropertyAsync(key).ConfigureAwait(false);
                if (handleItem is not null)
                {
                    dic.Add(key, handleItem);
                }
            }

            return dic;
        }

        /// <inheritdoc/>
        public async Task<object> JsonValueAsync() => await this.JsonValueAsync<object>().ConfigureAwait(false);

        /// <inheritdoc/>
        public abstract Task<T> JsonValueAsync<T>();

        /// <inheritdoc/>
        public abstract ValueTask DisposeAsync();

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateFunctionHandleAsync(string pageFunction, params object[] args)
        {
            return this.Realm.EvaluateFunctionHandleAsync(pageFunction, [this, .. args]);
        }

        /// <inheritdoc/>
        public async Task<JsonElement?> EvaluateFunctionAsync(string script, params object[] args)
        {
            var adoptedThis = await this.Realm.AdoptHandleAsync(this).ConfigureAwait(false);
            return await this.Realm.EvaluateFunctionAsync<JsonElement?>(script, [adoptedThis, .. args])
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            return this.Realm.EvaluateFunctionAsync<T>(script, [this, .. args]);
        }
    }
}
