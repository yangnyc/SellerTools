// <copyright file="CheckedState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageAccessibility
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Three-state boolean. See <seealso cref="SerializedAXNode.Checked"/> and. <seealso cref="SerializedAXNode.Pressed"/>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<CheckedState>))]
    public enum CheckedState
    {
        /// <summary>
        /// False.
        /// </summary>
        False = 0,

        /// <summary>
        /// True.
        /// </summary>
        True,

        /// <summary>
        /// Mixed.
        /// </summary>
        Mixed,
    }
}
