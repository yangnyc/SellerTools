// <copyright file="AccessibilityQueryAXTreeResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;
    using static PuppeteerSharp.Cdp.Messaging.AccessibilityGetFullAXTreeResponse;

    internal class AccessibilityQueryAXTreeResponse
    {
        public IEnumerable<AXTreeNode> Nodes { get; set; }
    }
}
