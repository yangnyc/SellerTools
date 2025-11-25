// <copyright file="TargetSetAutoAttachRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class TargetSetAutoAttachRequest
    {
        public bool AutoAttach { get; set; }

        public bool WaitForDebuggerOnStart { get; set; }

        public bool Flatten { get; set; }
    }
}
