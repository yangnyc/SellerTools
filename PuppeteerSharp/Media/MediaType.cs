// <copyright file="MediaType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Media
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Media type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<MediaType>))]
    public enum MediaType
    {
        /// <summary>
        /// Media Print.
        /// </summary>
        Print,

        /// <summary>
        /// Media Screen.
        /// </summary>
        Screen,

        /// <summary>
        /// No media set.
        /// </summary>
        [EnumMember(Value = "")]
        None,
    }
}
