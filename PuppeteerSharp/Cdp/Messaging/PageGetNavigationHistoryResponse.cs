// <copyright file="PageGetNavigationHistoryResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class PageGetNavigationHistoryResponse
    {
        public int CurrentIndex { get; set; }

        public List<HistoryEntry> Entries { get; set; }

        internal class HistoryEntry
        {
            public int Id { get; set; }
        }
    }
}
