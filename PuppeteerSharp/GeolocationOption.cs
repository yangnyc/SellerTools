// <copyright file="GeolocationOption.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Geolocation option.
    /// </summary>
    /// <seealso cref="IPage.SetGeolocationAsync(GeolocationOption)"/>
    public class GeolocationOption : IEquatable<GeolocationOption>
    {
        /// <summary>
        /// Gets or sets latitude between -90 and 90.
        /// </summary>
        /// <value>The latitude.</value>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets longitude between -180 and 180.
        /// </summary>
        /// <value>The longitude.</value>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Gets or sets optional non-negative accuracy value.
        /// </summary>
        /// <value>The accuracy.</value>
        public decimal Accuracy { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="PuppeteerSharp.GeolocationOption"/> is equal to the current <see cref="PuppeteerSharp.GeolocationOption"/>.
        /// </summary>
        /// <param name="other">The <see cref="PuppeteerSharp.GeolocationOption"/> to compare with the current <see cref="PuppeteerSharp.GeolocationOption"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="PuppeteerSharp.GeolocationOption"/> is equal to the current
        /// <see cref="PuppeteerSharp.GeolocationOption"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(GeolocationOption other)
            => other != null &&
                this.Latitude == other.Latitude &&
                this.Longitude == other.Longitude &&
                this.Accuracy == other.Accuracy;

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.Equals(obj as GeolocationOption);

        /// <inheritdoc/>
        public override int GetHashCode()
            => (this.Latitude.GetHashCode() ^ 2014) +
                (this.Longitude.GetHashCode() ^ 2014) +
                (this.Accuracy.GetHashCode() ^ 2014);
    }
}
