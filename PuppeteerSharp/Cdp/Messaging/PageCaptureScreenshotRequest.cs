// <copyright file="PageCaptureScreenshotRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using PuppeteerSharp.Media;

    internal class PageCaptureScreenshotRequest
    {
        public string Format { get; set; }

        public int Quality { get; set; }

        public Clip Clip { get; set; }

        public bool CaptureBeyondViewport { get; set; }

        public bool? FromSurface { get; set; }

        public bool? OptimizeForSpeed { get; set; }
    }
}
