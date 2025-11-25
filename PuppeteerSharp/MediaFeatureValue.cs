// <copyright file="MediaFeatureValue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Media Feature. <see cref="IPage.EmulateMediaFeaturesAsync(System.Collections.Generic.IEnumerable{MediaFeatureValue})"/>.
    /// </summary>
    public class MediaFeatureValue
    {
        /// <summary>
        /// Gets or sets the CSS media feature name. Supported names are `'prefers-colors-scheme'` and `'prefers-reduced-motion'`.
        /// </summary>
        [JsonPropertyName("name")]
        public MediaFeature MediaFeature { get; set; }

        /// <summary>
        /// Gets or sets the value for the given CSS media feature.
        /// </summary>
        public string Value { get; set; }
    }
}
