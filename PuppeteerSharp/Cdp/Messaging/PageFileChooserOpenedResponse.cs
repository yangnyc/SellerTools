// <copyright file="PageFileChooserOpenedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class PageFileChooserOpenedResponse
    {
        public string Mode { get; set; }

        public string FrameId { get; set; }

        public object BackendNodeId { get; set; }
    }
}
