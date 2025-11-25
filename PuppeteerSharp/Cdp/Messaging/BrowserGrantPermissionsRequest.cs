// <copyright file="BrowserGrantPermissionsRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class BrowserGrantPermissionsRequest
    {
        public string Origin { get; set; }

        public string BrowserContextId { get; set; }

        public OverridePermission[] Permissions { get; set; }
    }
}
