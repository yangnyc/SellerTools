// <copyright file="DomResolveNodeRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class DomResolveNodeRequest
    {
        public object BackendNodeId { get; set; }

        public int ExecutionContextId { get; set; }
    }
}
