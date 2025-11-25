// <copyright file="TransportClosedEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Transport
{
    using System;

    /// <summary>
    /// <see cref="IConnectionTransport.Closed"/>.
    /// </summary>
    public class TransportClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PuppeteerSharp.Transport.TransportClosedEventArgs"/> class.
        /// </summary>
        /// <param name="closeReason">Close reason.</param>
        public TransportClosedEventArgs(string closeReason) => this.CloseReason = closeReason;

        /// <summary>
        /// Gets or sets the close reason.
        /// </summary>
        public string CloseReason { get; set; }
    }
}
