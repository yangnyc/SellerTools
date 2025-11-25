// <copyright file="NetworkGetResponseBodyResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class NetworkGetResponseBodyResponse
    {
        public string Body { get; set; }

        public bool Base64Encoded { get; set; }
    }
}
