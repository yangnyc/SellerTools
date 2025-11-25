// <copyright file="TargetDetachedFromTargetResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetDetachedFromTargetResponse
    {
        public string SessionId { get; set; }

        public string TargetId { get; set; }
    }
}
