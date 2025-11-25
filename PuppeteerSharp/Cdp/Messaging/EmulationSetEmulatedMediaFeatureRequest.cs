// <copyright file="EmulationSetEmulatedMediaFeatureRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class EmulationSetEmulatedMediaFeatureRequest
    {
        public IEnumerable<MediaFeatureValue> Features { get; set; }
    }
}
