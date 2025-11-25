// <copyright file="AccessibilityQueryAXTreeRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class AccessibilityQueryAXTreeRequest
    {
        public string ObjectId { get; set; }

        public string AccessibleName { get; set; }

        public string Role { get; set; }
    }
}
