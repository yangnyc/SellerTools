// <copyright file="CookiePriority.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

/// <summary>
/// Represents the cookie's 'Priority' status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter<CookiePriority>))]
public enum CookiePriority
{
    /// <summary>
    /// Low priority.
    /// </summary>
    Low,

    /// <summary>
    /// Medium priority.
    /// </summary>
    Medium,

    /// <summary>
    /// High priority.
    /// </summary>
    High,
}
