// <copyright file="LogSource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging;

using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

[JsonConverter(typeof(JsonStringEnumMemberConverter<LogSource>))]
internal enum LogSource
{
    Xml = 0,
    Javascript,
    Network,
    Storage,
    Appcache,
    Rendering,
    Security,
    Deprecation,
    Worker,
    Violation,
    Intervention,
    Recommendation,
    Other,
}
