// <copyright file="QueuedEventGroup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using PuppeteerSharp.Cdp.Messaging;

    internal class QueuedEventGroup
    {
        public ResponseReceivedResponse ResponseReceivedEvent { get; set; }

        public LoadingFinishedEventResponse LoadingFinishedEvent { get; set; }

        public LoadingFailedEventResponse LoadingFailedEvent { get; set; }
    }
}
