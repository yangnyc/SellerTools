// <copyright file="DialogType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Dialog type.
    /// </summary>
    /// <seealso cref="Dialog"/>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<DialogType>))]
    public enum DialogType
    {
        /// <summary>
        /// Alert dialog.
        /// </summary>
        Alert,

        /// <summary>
        /// Prompt dialog.
        /// </summary>
        Prompt,

        /// <summary>
        /// Confirm dialog.
        /// </summary>
        Confirm,

        /// <summary>
        /// Before unload dialog.
        /// </summary>
        [EnumMember(Value = "beforeunload")]
        BeforeUnload,
    }
}
