// <copyright file="MultiMap.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class MultiMap<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ICollection<TValue>> map = new ConcurrentDictionary<TKey, ICollection<TValue>>();

        internal void Add(TKey key, TValue value)
            => this.map.GetOrAdd(key, _ => new ConcurrentSet<TValue>()).Add(value);

        internal ICollection<TValue> Get(TKey key)
            => this.map.TryGetValue(key, out var set) ? set : new ConcurrentSet<TValue>();

        internal bool Has(TKey key, TValue value)
            => this.map.TryGetValue(key, out var set) && set.Contains(value);

        internal bool Delete(TKey key, TValue value)
            => this.map.TryGetValue(key, out var set) && set.Remove(value);

        internal bool TryRemove(TKey key, out ICollection<TValue> value)
            => this.map.TryRemove(key, out value);

        internal TValue FirstValue(TKey key)
            => this.map.TryGetValue(key, out var set) ? set.FirstOrDefault() : default;

        internal void Clear()
            => this.map.Clear();
    }
}
