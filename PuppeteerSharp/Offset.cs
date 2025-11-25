// <copyright file="Offset.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Offset used in conjunction with <see cref="ElementHandle.ClickablePointAsync(Offset?)"/>.
    /// </summary>
    public struct Offset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Offset"/> struct.
        /// </summary>
        /// <param name="x">x-offset for the clickable point relative to the top-left corner of the border box.</param>
        /// <param name="y">y-offset for the clickable point relative to the top-left corner of the border box.</param>
        public Offset(decimal x, decimal y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or sets x-offset for the clickable point relative to the top-left corner of the border box.
        /// </summary>
        public decimal X { get; set; }

        /// <summary>
        /// Gets or sets y-offset for the clickable point relative to the top-left corner of the border box.
        /// </summary>
        public decimal Y { get; set; }
    }
}
