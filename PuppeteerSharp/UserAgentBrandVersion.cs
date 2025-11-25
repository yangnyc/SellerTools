// <copyright file="UserAgentBrandVersion.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Used to specify User Agent Cient Hints to emulate. See https://wicg.github.io/ua-client-hints.
    /// </summary>
    public class UserAgentBrandVersion
    {
        /// <summary>
        /// Gets or sets brand.
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Gets or sets version.
        /// </summary>
        public string Version { get; set; }
    }
}
