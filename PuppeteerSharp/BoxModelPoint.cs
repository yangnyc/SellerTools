// <copyright file="BoxModelPoint.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Represents a point.
    /// </summary>
    public record BoxModelPoint
    {
        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        public decimal X { get; set; }

        /// <summary>
        /// Gets or sets the y coordinate.
        /// </summary>
        public decimal Y { get; set; }
    }
}
