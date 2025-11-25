// <copyright file="LifecycleEventResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class LifecycleEventResponse
    {
        public string FrameId { get; set; }

        public string LoaderId { get; set; }

        public string Name { get; set; }
    }
}
