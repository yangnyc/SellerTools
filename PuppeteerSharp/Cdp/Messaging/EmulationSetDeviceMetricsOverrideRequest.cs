// <copyright file="EmulationSetDeviceMetricsOverrideRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using PuppeteerSharp.Media;

    internal class EmulationSetDeviceMetricsOverrideRequest
    {
        public bool Mobile { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public double DeviceScaleFactor { get; set; }

        public ScreenOrientation ScreenOrientation { get; set; }
    }
}
