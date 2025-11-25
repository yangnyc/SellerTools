// <copyright file="PointerType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Input
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<PointerType>))]
    internal enum PointerType
    {
        /// <summary>
        /// Mouse.
        /// </summary>
        [EnumMember(Value = "mouse")]
        Mouse,

        /// <summary>
        /// Pen.
        /// </summary>
        [EnumMember(Value = "pen")]
        Pen,
    }
}
