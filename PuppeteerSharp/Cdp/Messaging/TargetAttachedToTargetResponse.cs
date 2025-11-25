// <copyright file="TargetAttachedToTargetResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetAttachedToTargetResponse
    {
        public TargetInfo TargetInfo { get; set; }

        public string SessionId { get; set; }
    }
}
