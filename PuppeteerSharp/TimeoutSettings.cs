// <copyright file="TimeoutSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Timeout settings.
    /// </summary>
    public class TimeoutSettings
    {
        private int? defaultNavigationTimeout;

        /// <summary>
        /// Gets or sets navigation Timeout.
        /// </summary>
        public int NavigationTimeout
        {
            get => this.defaultNavigationTimeout ?? this.Timeout;
            set => this.defaultNavigationTimeout = value;
        }

        /// <summary>
        /// Gets or sets default timeout.
        /// </summary>
        public int Timeout { get; set; } = Puppeteer.DefaultTimeout;
    }
}
