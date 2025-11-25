// <copyright file="FetchFailRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class FetchFailRequest
    {
        public string RequestId { get; set; }

        public string ErrorReason { get; set; }
    }
}
