// <copyright file="InputDispatchTouchEventRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using PuppeteerSharp.Input;

    internal class InputDispatchTouchEventRequest
    {
        public string Type { get; internal set; }

        public TouchPoint[] TouchPoints { get; set; }

        public int Modifiers { get; internal set; }
    }
}
