// <copyright file="AnyTypeToStringConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers.Json;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class AnyTypeToStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? string.Empty;
        }

        // There could be many types by we assume that, in general, this will be used for ints.
        if (reader.TokenType == JsonTokenType.Number)
        {
            var stringValue = reader.GetInt32();
            return stringValue.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        if (reader.TokenType is JsonTokenType.False or JsonTokenType.True)
        {
            return reader.GetBoolean().ToString();
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            reader.Skip();
            return "(not supported)";
        }

        throw new JsonException($"Unsupported token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (int.TryParse(value, out var number))
        {
            writer.WriteNumberValue(number);
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
