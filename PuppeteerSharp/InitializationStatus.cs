// <copyright file="InitializationStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    [JsonConverter(typeof(JsonStringEnumMemberConverter<InitializationStatus>))]
    internal enum InitializationStatus
    {
        Aborted,
        Success,
    }
}
