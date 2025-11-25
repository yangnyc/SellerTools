// <copyright file="BoundingBox.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using PuppeteerSharp.Media;

    /// <summary>
    /// Bounding box data returned by <see cref="IElementHandle.BoundingBoxAsync"/>.
    /// </summary>
    public class BoundingBox : IEquatable<BoundingBox>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> class.
        /// </summary>
        public BoundingBox()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public BoundingBox(decimal x, decimal y, decimal width, decimal height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets or sets the x coordinate of the element in pixels.
        /// </summary>
        /// <value>The x.</value>
        public decimal X { get; set; }

        /// <summary>
        /// Gets or sets the y coordinate of the element in pixels.
        /// </summary>
        /// <value>The y.</value>
        public decimal Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the element in pixels.
        /// </summary>
        /// <value>The width.</value>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the element in pixels.
        /// </summary>
        /// <value>The height.</value>
        public decimal Height { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((BoundingBox)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="PuppeteerSharp.BoundingBox"/> is equal to the current <see cref="PuppeteerSharp.BoundingBox"/>.
        /// </summary>
        /// <param name="obj">The <see cref="PuppeteerSharp.BoundingBox"/> to compare with the current <see cref="PuppeteerSharp.BoundingBox"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="PuppeteerSharp.BoundingBox"/> is equal to the current
        /// <see cref="PuppeteerSharp.BoundingBox"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(BoundingBox obj)
            => obj != null &&
                obj.X == this.X &&
                obj.Y == this.Y &&
                obj.Height == this.Height &&
                obj.Width == this.Width;

        /// <inheritdoc/>
        public override int GetHashCode()
            => this.X.GetHashCode() * 397
                ^ this.Y.GetHashCode() * 397
                ^ this.Width.GetHashCode() * 397
                ^ this.Height.GetHashCode() * 397;

        internal Clip ToClip()
        {
            return new Clip
            {
                X = this.X,
                Y = this.Y,
                Width = this.Width,
                Height = this.Height,
            };
        }
    }
}
