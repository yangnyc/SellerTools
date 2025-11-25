// <copyright file="PageFrameNavigatedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging;

internal class PageFrameNavigatedResponse
{
    public FramePayload Frame { get; set; }

    public NavigationType Type { get; set; }
}
