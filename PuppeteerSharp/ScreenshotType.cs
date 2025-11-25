// <copyright file="ScreenshotType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Screenshot file type.
    /// </summary>
    /// <seealso cref="ScreenshotOptions"/>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<ScreenshotType>))]
    public enum ScreenshotType
    {
        /// <summary>
        /// JPEG type.
        /// </summary>
        Jpeg,

        /// <summary>
        /// PNG type.
        /// </summary>
        Png,

        /// <summary>
        /// Webp type.
        /// </summary>
        Webp,
    }
}
