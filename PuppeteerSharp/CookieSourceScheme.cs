// <copyright file="CookieSourceScheme.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

/// <summary>
/// Represents the source scheme of the origin that originally set the cookie. A value of
/// "Unset" allows protocol clients to emulate legacy cookie scope for the scheme.
/// This is a temporary ability and it will be removed in the future.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter<CookieSourceScheme>))]
public enum CookieSourceScheme
{
    /// <summary>
    /// Unset.
    /// </summary>
    Unset,

    /// <summary>
    /// Non-secure.
    /// </summary>
    NonSecure,

    /// <summary>
    /// Secure.
    /// </summary>
    Secure,
}
