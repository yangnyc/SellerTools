// <copyright file="TargetCreateTargetRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetCreateTargetRequest
    {
        public string Url { get; set; }

        public object BrowserContextId { get; set; }
    }
}
