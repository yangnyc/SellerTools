// <copyright file="FetchEnableRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class FetchEnableRequest
    {
        public bool HandleAuthRequests { get; set; }

        public Pattern[] Patterns { get; set; }

        internal class Pattern(string urlPattern)
        {
            public string UrlPattern { get; set; } = urlPattern;
        }
    }
}
