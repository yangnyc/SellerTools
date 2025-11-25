// <copyright file="RemoteObjectType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Remote object type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<RemoteObjectType>))]
    [DefaultEnumValue((int)Other)]
    public enum RemoteObjectType
    {
        /// <summary>
        /// Other.
        /// </summary>
        Other,

        /// <summary>
        /// Object.
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
        Object,
#pragma warning restore CA1720 // Identifier contains type name
        /// <summary>
        /// Function.
        /// </summary>
        Function,

        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined,

        /// <summary>
        /// String.
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
        String,
#pragma warning restore CA1720 // Identifier contains type name
        /// <summary>
        /// Number.
        /// </summary>
        Number,

        /// <summary>
        /// Boolean.
        /// </summary>
        Boolean,

        /// <summary>
        /// Symbol.
        /// </summary>
        Symbol,

        /// <summary>
        /// Bigint.
        /// </summary>
        Bigint,
    }
}
