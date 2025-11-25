// <copyright file="DomGetBoxModelRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class DomGetBoxModelRequest
    {
        public string ObjectId { get; set; }

        public object BackendNodeId { get; set; }
    }
}
