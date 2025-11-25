// <copyright file="TargetAttachToTargetRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetAttachToTargetRequest
    {
        public string TargetId { get; set; }

        public bool Flatten { get; set; }
    }
}
