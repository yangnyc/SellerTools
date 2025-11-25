// <copyright file="PageFrameDetachedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<FrameDetachedReason>))]
    internal enum FrameDetachedReason
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Remove.
        /// </summary>
        Remove,

        /// <summary>
        /// Swap.
        /// </summary>
        Swap,
    }

    internal class PageFrameDetachedResponse : BasicFrameResponse
    {
        public FrameDetachedReason Reason { get; set; }
    }
}
