// <copyright file="RemoteAddress.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Remote server address.
    /// </summary>
    /// <seealso cref="IResponse.RemoteAddress"/>
    public class RemoteAddress
    {
        /// <summary>
        /// Gets or sets the IP address of the remote server.
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Gets or sets the port used to connect to the remote server.
        /// </summary>
        public int Port { get; set; }
    }
}
