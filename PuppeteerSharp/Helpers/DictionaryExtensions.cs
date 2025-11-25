// <copyright file="DictionaryExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        internal static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dic)
            => new(dic, dic.Comparer);

        // GetValueOrDefault is available in .NET 8, but we also target .NETStandard
        internal static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out var ret);
            return ret;
        }
    }
}
