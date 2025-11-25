// <copyright file="PageGetFrameTree.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class PageGetFrameTree
    {
        public FramePayload Frame { get; set; }

        public PageGetFrameTree[] ChildFrames { get; set; }
    }
}
