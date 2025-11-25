// <copyright file="ResponseReceivedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class ResponseReceivedResponse
    {
        public string RequestId { get; set; }

        public ResponsePayload Response { get; set; }

        public bool HasExtraInfo { get; set; }
    }
}
