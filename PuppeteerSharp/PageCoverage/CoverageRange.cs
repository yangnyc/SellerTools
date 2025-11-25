// <copyright file="CoverageRange.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    /// <summary>
    /// Coverage data for a source range.
    /// </summary>
    public record CoverageRange
    {
        /// <summary>
        /// Gets or sets javaScript script source offset for the range start.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// Gets or sets javaScript script source offset for the range end.
        /// </summary>
        public int EndOffset { get; set; }

        /// <summary>
        /// Gets or sets collected execution count of the source range.
        /// </summary>
        public int Count { get; set; }
    }
}
