// <copyright file="Clip.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Media
{
    /// <summary>
    /// Clip data.
    /// </summary>
    /// <seealso cref="ScreenshotOptions.Clip"/>
    public class Clip : BoundingBox
    {
        /// <summary>
        /// Gets or sets scale of the webpage rendering. Defaults to 1.
        /// </summary>
        /// <value>The scale.</value>
        public int Scale { get; set; } = 1;
    }
}
