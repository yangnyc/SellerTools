// <copyright file="FirefoxChannel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.BrowserData;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

/// <summary>
/// Firefox channels.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter<FirefoxChannel>))]
public enum FirefoxChannel
{
    /// <summary>
    /// Stable.
    /// </summary>
    [EnumMember(Value = "stable")]
    Stable = 0,

    /// <summary>
    /// Beta.
    /// </summary>
    [EnumMember(Value = "beta")]
    Beta = 1,

    /// <summary>
    /// Nightly.
    /// </summary>
    [EnumMember(Value = "nightly")]
    Nightly = 2,

    /// <summary>
    /// ESR.
    /// </summary>
    [EnumMember(Value = "esr")]
    Esr = 3,

    /// <summary>
    /// Developer Edition.
    /// </summary>
    [EnumMember(Value = "devedition")]
    DevEdition = 4,
}
