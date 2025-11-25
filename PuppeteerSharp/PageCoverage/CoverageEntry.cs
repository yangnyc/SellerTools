// <copyright file="CoverageEntry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    /// <summary>
    /// Coverage report for all non-anonymous scripts.
    /// </summary>
    public record CoverageEntry
    {
        /// <summary>
        /// Gets or sets script URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets script ranges that were executed. Ranges are sorted and non-overlapping.
        /// </summary>
        public CoverageEntryRange[] Ranges { get; set; }

        /// <summary>
        /// Gets or sets script content.
        /// </summary>
        public string Text { get; set; }
    }
}
