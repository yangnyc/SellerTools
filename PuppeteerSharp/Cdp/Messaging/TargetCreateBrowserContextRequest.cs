// <copyright file="TargetCreateBrowserContextRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetCreateBrowserContextRequest
    {
        public string ProxyServer { get; set; }

        public string ProxyBypassList { get; set; }
    }
}
