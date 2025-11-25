// <copyright file="DragEventType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<DragEventType>))]
    internal enum DragEventType
    {
        /// <summary>
        /// Drag event.
        /// </summary>
        [EnumMember(Value = "dragEnter")]
        DragEnter,

        /// <summary>
        /// Drag over.
        /// </summary>
        [EnumMember(Value = "dragOver")]
        DragOver,

        /// <summary>
        /// Drop.
        /// </summary>
        [EnumMember(Value = "drop")]
        Drop,
    }
}
