// <copyright file="BoxModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Represents boxes of the element.
    /// </summary>
    public class BoxModel
    {
        /// <summary>
        /// Gets or sets the Content box.
        /// </summary>
        public BoxModelPoint[] Content { get; set; }

        /// <summary>
        /// Gets or sets the Padding box.
        /// </summary>
        public BoxModelPoint[] Padding { get; set; }

        /// <summary>
        /// Gets or sets the Border box.
        /// </summary>
        public BoxModelPoint[] Border { get; set; }

        /// <summary>
        /// Gets or sets the Margin box.
        /// </summary>
        public BoxModelPoint[] Margin { get; set; }

        /// <summary>
        /// Gets or sets the element's width.
        /// </summary>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets the element's height.
        /// </summary>
        public decimal Height { get; set; }
    }
}
