// <copyright file="ContextPayloadAuxData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    internal class ContextPayloadAuxData
    {
        public string FrameId { get; set; }

        public bool IsDefault { get; set; }

        public DOMWorldType Type { get; set; }
    }
}
