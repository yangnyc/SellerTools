// <copyright file="CdpTouchscreen.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Input;

/// <inheritdoc/>
public class CdpTouchscreen : Touchscreen
{
    private readonly Keyboard keyboard;
    private CDPSession client;

    /// <inheritdoc cref="Touchscreen"/>
    internal CdpTouchscreen(CDPSession client, Keyboard keyboard)
    {
        this.client = client;
        this.keyboard = keyboard;
    }

    /// <inheritdoc />
    public override Task TouchStartAsync(decimal x, decimal y)
    {
        var touchPoints = new[]
        {
                new TouchPoint
                {
                    X = Math.Round(x, MidpointRounding.AwayFromZero),
                    Y = Math.Round(y, MidpointRounding.AwayFromZero),
                    RadiusX = 0.5m,
                    RadiusY = 0.5m,
                    Force = 0.5m,
                },
        };

        return this.client.SendAsync(
            "Input.dispatchTouchEvent",
            new InputDispatchTouchEventRequest
            {
                Type = "touchStart",
                TouchPoints = touchPoints,
                Modifiers = this.keyboard.Modifiers,
            });
    }

    /// <inheritdoc />
    public override Task TouchMoveAsync(decimal x, decimal y)
    {
        var touchPoints = new[]
        {
                new TouchPoint
                {
                    X = Math.Round(x, MidpointRounding.AwayFromZero),
                    Y = Math.Round(y, MidpointRounding.AwayFromZero),
                    RadiusX = 0.5m,
                    RadiusY = 0.5m,
                    Force = 0.5m,
                },
        };

        return this.client.SendAsync(
            "Input.dispatchTouchEvent",
            new InputDispatchTouchEventRequest
            {
                Type = "touchStart",
                TouchPoints = touchPoints,
                Modifiers = this.keyboard.Modifiers,
            });
    }

    /// <inheritdoc />
    public override Task TouchEndAsync()
    {
        return this.client.SendAsync(
            "Input.dispatchTouchEvent",
            new InputDispatchTouchEventRequest
            {
                Type = "touchEnd",
                TouchPoints = [],
                Modifiers = this.keyboard.Modifiers,
            });
    }

    internal void UpdateClient(CDPSession newSession) => this.client = newSession;
}
