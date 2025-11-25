// <copyright file="DomDescribeNodeResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json;

    internal class DomDescribeNodeResponse
    {
        public DomNode Node { get; set; }

        internal class DomNode
        {
            public string FrameId { get; set; }

            public JsonElement BackendNodeId { get; set; }
        }
    }
}
