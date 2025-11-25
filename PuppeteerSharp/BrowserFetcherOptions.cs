// <copyright file="BrowserFetcherOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Threading.Tasks;

    /// <summary>
    /// Browser fetcher options used to construct a <see cref="BrowserFetcher"/>.
    /// </summary>
    public class BrowserFetcherOptions
    {
        /// <summary>
        /// A custom download delegate.
        /// </summary>
        /// <param name="address">address.</param>
        /// <param name="fileName">fileName.</param>
        /// <returns>A Task that resolves when the download finishes.</returns>
        public delegate Task CustomFileDownloadAction(string address, string fileName);

        /// <summary>
        /// Gets or sets browser. Defaults to Chrome.
        /// </summary>
        public SupportedBrowser Browser { get; set; } = SupportedBrowser.Chrome;

        /// <summary>
        /// Gets or sets platform. Defaults to current platform.
        /// </summary>
        public Platform? Platform { get; set; }

        /// <summary>
        /// Gets or sets a path for the downloads folder. Defaults to [root]/.local-chromium, where [root] is where the project binaries are located.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a download host to be used. Defaults to https://storage.googleapis.com.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the default or a custom download delegate.
        /// </summary>
        public CustomFileDownloadAction CustomFileDownload { get; set; }
    }
}
