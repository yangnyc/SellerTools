// <copyright file="NavigationType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging;

using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

/// <summary>
/// Navigation types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter<NavigationType>))]
public enum NavigationType
{
    /// <summary>
    /// Normal navigation.
    /// </summary>
    Navigation,

    /// <summary>
    /// Back forward cache restore.
    /// </summary>
    BackForwardCacheRestore,
}
