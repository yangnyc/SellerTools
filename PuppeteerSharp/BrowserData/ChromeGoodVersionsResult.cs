// <copyright file="ChromeGoodVersionsResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.BrowserData
{
    using System.Collections.Generic;

    internal class ChromeGoodVersionsResult
    {
        public Dictionary<string, ChromeGoodVersionsResultVersion> Channels { get; set; }

        internal class ChromeGoodVersionsResultVersion
        {
            public string Version { get; set; }
        }
    }
}
