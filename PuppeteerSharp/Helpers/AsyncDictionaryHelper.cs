// <copyright file="AsyncDictionaryHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    internal class AsyncDictionaryHelper<TKey, TValue>
    {
        private readonly string timeoutMessage;
        private readonly MultiMap<TKey, TaskCompletionSource<TValue>> pendingRequests = new();
        private readonly ConcurrentDictionary<TKey, TValue> dictionary = new();

        public AsyncDictionaryHelper(string timeoutMessage)
        {
            this.timeoutMessage = timeoutMessage;
        }

        internal ICollection<TValue> Values => this.dictionary.Values;

        internal async Task<TValue> GetItemAsync(TKey key)
        {
            var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.pendingRequests.Add(key, tcs);

            if (this.dictionary.TryGetValue(key, out var item))
            {
                this.pendingRequests.Delete(key, tcs);
                return item;
            }

            return await tcs.Task.WithTimeout(
                new Action(() =>
                    throw new PuppeteerException(string.Format(CultureInfo.CurrentCulture, this.timeoutMessage, key))),
                1000).ConfigureAwait(false);
        }

        internal async Task<TValue> TryGetItemAsync(TKey key)
        {
            var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.pendingRequests.Add(key, tcs);

            if (this.dictionary.TryGetValue(key, out var item))
            {
                this.pendingRequests.Delete(key, tcs);
                return item;
            }

            return await tcs.Task.WithTimeout(() => { }, 1000).ConfigureAwait(false);
        }

        internal void AddItem(TKey key, TValue value)
        {
            this.dictionary[key] = value;
            foreach (var tcs in this.pendingRequests.Get(key))
            {
                tcs.TrySetResult(value);
            }
        }

        internal bool TryRemove(TKey key, out TValue value)
        {
            var result = this.dictionary.TryRemove(key, out value);
            _ = this.pendingRequests.TryRemove(key, out _);
            return result;
        }

        internal void Clear()
        {
            this.dictionary.Clear();
            this.pendingRequests.Clear();
        }

        internal TValue GetValueOrDefault(TKey key)
            => this.dictionary.GetValue(key);

        internal bool TryGetValue(TKey key, out TValue value)
            => this.dictionary.TryGetValue(key, out value);

        internal bool ContainsKey(TKey key)
            => this.dictionary.ContainsKey(key);
    }
}
