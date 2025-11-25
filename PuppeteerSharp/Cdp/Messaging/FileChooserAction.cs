// <copyright file="FileChooserAction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<FileChooserAction>))]
    internal enum FileChooserAction
    {
        /// <summary>
        /// Accept.
        /// </summary>
        [EnumMember(Value = "accept")]
        Accept,

        /// <summary>
        /// Fallback.
        /// </summary>
        [EnumMember(Value = "fallback")]
        Fallback,

        /// <summary>
        /// Cancel.
        /// </summary>
        [EnumMember(Value = "cancel")]
        Cancel,
    }
}
