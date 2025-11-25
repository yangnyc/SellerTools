// <copyright file="NetworkSetExtraHTTPHeadersRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class NetworkSetExtraHTTPHeadersRequest(Dictionary<string, string> headers)
    {
        public Dictionary<string, string> Headers { get; set; } = headers;
    }
}
