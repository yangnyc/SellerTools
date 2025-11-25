// <copyright file="MouseEventType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<MouseEventType>))]
    internal enum MouseEventType
    {
        /// <summary>
        /// Mouse moved.
        /// </summary>
        [EnumMember(Value = "mouseMoved")]
        MouseMoved,

        /// <summary>
        /// Mouse clicked.
        /// </summary>
        [EnumMember(Value = "mousePressed")]
        MousePressed,

        /// <summary>
        /// Mouse click released.
        /// </summary>
        [EnumMember(Value = "mouseReleased")]
        MouseReleased,

        /// <summary>
        /// Mouse wheel.
        /// </summary>
        [EnumMember(Value = "mouseWheel")]
        MouseWheel,
    }
}
