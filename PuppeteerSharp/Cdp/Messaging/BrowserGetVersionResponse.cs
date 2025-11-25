// <copyright file="BrowserGetVersionResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class BrowserGetVersionResponse
    {
        public string UserAgent { get; set; }

        public string Product { get; set; }
    }
}
