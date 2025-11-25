// <copyright file="NetworkSetUserAgentOverrideRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class NetworkSetUserAgentOverrideRequest
    {
        public string UserAgent { get; set; }

        public UserAgentMetadata UserAgentMetadata { get; set; }
    }
}
