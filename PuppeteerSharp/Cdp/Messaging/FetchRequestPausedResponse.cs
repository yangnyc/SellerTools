// <copyright file="FetchRequestPausedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class FetchRequestPausedResponse : RequestWillBeSentResponse
    {
        public ResourceType? ResourceType { get; set; }
    }
}
