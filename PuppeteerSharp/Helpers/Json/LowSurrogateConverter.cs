// <copyright file="LowSurrogateConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers.Json;

using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class LowSurrogateConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return null;
        }

        var span = reader.HasValueSequence
            ? reader.ValueSequence.ToArray()
#if NET8_0_OR_GREATER
            : reader.ValueSpan;
#else
            : reader.ValueSpan.ToArray();
#endif
        var value = Encoding.UTF8.GetString(span);

        try
        {
            return JsonUnescapedValue(reader, value);
        }
        catch
        {
            return value;
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    private static string JsonUnescapedValue(Utf8JsonReader reader, string jsonString)
    {
        if (reader.ValueIsEscaped)
        {
            using var doc = JsonDocument.Parse($"\"{jsonString}\"");
            return doc.RootElement.GetString();
        }
        else
        {
            return jsonString;
        }
    }
}
