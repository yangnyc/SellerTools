// <copyright file="NetworkSetCacheDisabledRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class NetworkSetCacheDisabledRequest(bool cacheDisabled)
    {
        public bool CacheDisabled { get; set; } = cacheDisabled;
    }
}
