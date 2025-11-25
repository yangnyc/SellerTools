// <copyright file="UserAgentMetadata.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    ///  Used to specify User Agent Cient Hints to emulate. See https://wicg.github.io/ua-client-hints
    ///  Missing optional values will be filled in by the target with what it would normally use.
    /// </summary>
    public class UserAgentMetadata
    {
        /// <summary>
        /// Gets or sets brands.
        /// </summary>
        public UserAgentBrandVersion[] Brands { get; set; }

        /// <summary>
        /// Gets or sets full version.
        /// </summary>
        public string FullVersion { get; set; }

        /// <summary>
        /// Gets or sets platform.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets platform version.
        /// </summary>
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Gets or sets architecture.
        /// </summary>
        public string Architecture { get; set; }

        /// <summary>
        /// Gets or sets model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mobile.
        /// </summary>
        public bool Mobile { get; set; }
    }
}
