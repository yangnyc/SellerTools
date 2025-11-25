// <copyright file="DispatchKeyEventType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<DispatchKeyEventType>))]
    internal enum DispatchKeyEventType
    {
        /// <summary>
        /// Key down.
        /// </summary>
        [EnumMember(Value = "keyDown")]
        KeyDown,

        /// <summary>
        /// Raw key down.
        /// </summary>
        [EnumMember(Value = "rawKeyDown")]
        RawKeyDown,

        /// <summary>
        /// Key up.
        /// </summary>
        [EnumMember(Value = "keyUp")]
        KeyUp,
    }
}
