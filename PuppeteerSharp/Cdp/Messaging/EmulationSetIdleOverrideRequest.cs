// <copyright file="EmulationSetIdleOverrideRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class EmulationSetIdleOverrideRequest
    {
        public bool IsUserActive { get; set; }

        public bool IsScreenUnlocked { get; set; }
    }
}
