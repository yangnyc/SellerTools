// <copyright file="BrowserContextOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// BrowserContext options.
    /// </summary>
    public class BrowserContextOptions
    {
        /// <summary>
        /// Gets or sets proxy server with optional port to use for all requests.
        /// Username and password can be set in <see cref="IPage.AuthenticateAsync(Credentials)"/>.
        /// </summary>
        public string ProxyServer { get; set; }

        /// <summary>
        /// Gets or sets bypass the proxy for the given semi-colon-separated list of hosts.
        /// </summary>
        public string[] ProxyBypassList { get; set; }
    }
}
