// <copyright file="IOReadResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class IOReadResponse
    {
        public bool Eof { get; set; }

        public string Data { get; set; }

        public bool Base64Encoded { get; set; }
    }
}
