// <copyright file="SameSite.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// SameSite values in cookies.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<SameSite>))]
    [DefaultEnumValue((int)None)]
    public enum SameSite
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Strict.
        /// </summary>
        Strict,

        /// <summary>
        /// Lax.
        /// </summary>
        Lax,

        /// <summary>
        /// Extended.
        /// </summary>
        Extended,
    }
}
