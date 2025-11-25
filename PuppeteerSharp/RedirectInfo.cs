// <copyright file="RedirectInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using PuppeteerSharp.Cdp.Messaging;

    internal class RedirectInfo
    {
        public RequestWillBeSentResponse Event { get; set; }

        public string FetchRequestId { get; set; }
    }
}
