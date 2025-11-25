// <copyright file="SessionEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Session event arguments.
    /// </summary>
    public class SessionEventArgs
    {
        internal SessionEventArgs(ICDPSession session)
        {
            this.Session = session;
        }

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        public ICDPSession Session { get; set; }
    }
}
