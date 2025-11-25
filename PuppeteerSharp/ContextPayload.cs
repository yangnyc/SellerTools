// <copyright file="ContextPayload.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    internal class ContextPayload
    {
        public int Id { get; set; }

        public ContextPayloadAuxData AuxData { get; set; }

        public string Name { get; set; }
    }
}
