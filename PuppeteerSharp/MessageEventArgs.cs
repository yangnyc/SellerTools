// <copyright file="MessageEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Text.Json;

    /// <summary>
    /// <seealso cref="CDPSession.MessageReceived"/> arguments.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageID { get; internal set; }

        /// <summary>
        /// Gets the message data.
        /// </summary>
        /// <value>The message data.</value>
        public JsonElement MessageData { get; internal set; }
    }
}
