// <copyright file="TracingStartRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TracingStartRequest
    {
        public string Categories { get; set; }

        public string TransferMode { get; set; }
    }
}
