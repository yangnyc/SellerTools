// <copyright file="WaitTaskTimeoutException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Timeout exception that might be thrown by <c>WaitFor</c> methods in <see cref="Frame"/>.
    /// </summary>
    [Serializable]
    public class WaitTaskTimeoutException : PuppeteerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskTimeoutException"/> class.
        /// </summary>
        public WaitTaskTimeoutException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskTimeoutException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public WaitTaskTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskTimeoutException"/> class.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public WaitTaskTimeoutException(int timeout)
            : base($"Waiting failed: {timeout}ms exceeded")
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskTimeoutException"/> class.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        /// <param name="elementType">Element type.</param>
        public WaitTaskTimeoutException(int timeout, string elementType)
            : base($"waiting for {elementType} failed: timeout {timeout}ms exceeded")
        {
            this.Timeout = timeout;
            this.ElementType = elementType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskTimeoutException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public WaitTaskTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets timeout that caused the exception.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout { get; }

        /// <summary>
        /// Gets element type the WaitTask was waiting for.
        /// </summary>
        /// <value>The element.</value>
        public string ElementType { get; }
    }
}
