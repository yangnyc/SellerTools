// <copyright file="EmulationSetTouchEmulationEnabledRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class EmulationSetTouchEmulationEnabledRequest
    {
        public string Configuration { get; set; }

        public bool Enabled { get; set; }
    }
}
