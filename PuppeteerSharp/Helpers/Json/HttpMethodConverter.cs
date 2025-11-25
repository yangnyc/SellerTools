// <copyright file="HttpMethodConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers.Json;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// <see cref="HttpMethodConverter"/> will convert a <see cref="HttpMethod"/> to and from a string.
/// </summary>
internal sealed class HttpMethodConverter : JsonConverter<HttpMethod>
{
    /// <inheritdoc cref="JsonConverter"/>
    public override bool CanConvert(Type objectType) => typeof(HttpMethod).IsAssignableFrom(objectType);

    /// <inheritdoc cref="JsonConverter"/>
    public override HttpMethod Read(
        ref Utf8JsonReader reader,
        Type objectType,
        JsonSerializerOptions options)
    {
        var readerString = reader.GetString() ?? string.Empty;
        return new HttpMethod(readerString);
    }

    /// <inheritdoc cref="JsonConverter"/>
    public override void Write(
        Utf8JsonWriter writer,
        HttpMethod value,
        JsonSerializerOptions options)
    {
        if (value != null && writer != null)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
