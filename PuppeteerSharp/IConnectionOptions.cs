// <copyright file="IConnectionOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Net.WebSockets;
    using PuppeteerSharp.Cdp;
    using PuppeteerSharp.Transport;

    /// <summary>
    /// Options for <see cref="Connection"/> creation.
    /// </summary>
    public interface IConnectionOptions
    {
        /// <summary>
        /// Gets or sets slows down Puppeteer operations by the specified amount of milliseconds. Useful so that you can see what is going on.
        /// </summary>
        int SlowMo { get; set; }

        /// <summary>
        /// Gets or sets optional factory for <see cref="WebSocket"/> implementations.
        /// If <see cref="Transport"/> is set this property will be ignored.
        /// </summary>
        WebSocketFactory WebSocketFactory { get; set; }

        /// <summary>
        /// Gets or sets optional factory for <see cref="IConnectionTransport"/> implementations.
        /// </summary>
        TransportFactory TransportFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if not <see cref="Transport"/> is set this will be use to determine is the default <see cref="WebSocketTransport"/> will enqueue messages.
        /// </summary>
        bool EnqueueTransportMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether affects how responses to <see cref="CDPSession.SendAsync"/> are returned to the caller.
        /// </summary>
        bool EnqueueAsyncMessages { get; set; }

        /// <summary>
        /// Gets or sets callback to decide if Puppeteer should connect to a given target or not.
        /// </summary>
        public Func<Target, bool> TargetFilter { get; set; }

        /// <summary>
        /// Gets or sets timeout setting for individual protocol (CDP) calls.
        /// Defaults to 180_000.
        /// </summary>
        public int ProtocolTimeout { get; set; }
    }
}
