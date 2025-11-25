// <copyright file="PageFrameAttachedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class PageFrameAttachedResponse
    {
        public string FrameId { get; set; }

        public string ParentFrameId { get; set; }
    }
}
