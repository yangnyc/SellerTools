// <copyright file="TargetSendMessageToTargetRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetSendMessageToTargetRequest
    {
        public string SessionId { get; set; }

        public string Message { get; set; }
    }
}
