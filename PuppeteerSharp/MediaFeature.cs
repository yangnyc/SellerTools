// <copyright file="MediaFeature.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Media Feature. See <see cref="IPage.EmulateMediaFeaturesAsync(System.Collections.Generic.IEnumerable{MediaFeatureValue})"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<MediaFeature>))]
    public enum MediaFeature
    {
        /// <summary>
        /// prefers-color-scheme media feature.
        /// </summary>
        [EnumMember(Value = "prefers-color-scheme")]
        PrefersColorScheme,

        /// <summary>
        /// prefers-reduced-motion media feature.
        /// </summary>
        [EnumMember(Value = "prefers-reduced-motion")]
        PrefersReducedMotion,
    }
}
