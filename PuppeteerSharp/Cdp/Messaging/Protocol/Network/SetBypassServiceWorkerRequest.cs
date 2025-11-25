// <copyright file="SetBypassServiceWorkerRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging.Protocol.Network;

internal record SetBypassServiceWorkerRequest
{
    public bool Bypass { get; set; }
}
