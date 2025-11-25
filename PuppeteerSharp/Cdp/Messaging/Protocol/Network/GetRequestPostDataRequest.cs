// <copyright file="GetRequestPostDataRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging.Protocol.Network;

internal class GetRequestPostDataRequest(string requestId)
{
    public string RequestId { get; set; } = requestId;
}
