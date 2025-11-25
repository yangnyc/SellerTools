// <copyright file="MessageReceivedEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Transport
{
    using System;

    /// <summary>
    /// Message received event arguments.
    /// <see cref="IConnectionTransport.MessageReceived"/>.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PuppeteerSharp.Transport.MessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public MessageReceivedEventArgs(byte[] message) => this.Message = message;

        /// <summary>
        /// Gets transport message.
        /// </summary>
        public byte[] Message { get; }
    }
}
