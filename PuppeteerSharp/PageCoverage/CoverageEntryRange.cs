// <copyright file="CoverageEntryRange.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    /// <summary>
    /// Script range.
    /// </summary>
    public record CoverageEntryRange
    {
        /// <summary>
        /// Gets a start offset in text, inclusive.
        /// </summary>
        /// <value>Start offset.</value>
        public int Start { get; internal set; }

        /// <summary>
        /// Gets an end offset in text, exclusive.
        /// </summary>
        /// <value>End offset.</value>
        public int End { get; internal set; }
    }
}
