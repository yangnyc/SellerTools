// <copyright file="PageErrorEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Page error event arguments.
    /// </summary>
    public class PageErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageErrorEventArgs"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public PageErrorEventArgs(string message) => this.Message = message;

        /// <summary>
        /// Gets or sets error Message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}
