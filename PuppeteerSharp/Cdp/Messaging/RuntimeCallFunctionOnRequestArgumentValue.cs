// <copyright file="RuntimeCallFunctionOnRequestArgumentValue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging;

using System.Text.Json;

internal class RuntimeCallFunctionOnRequestArgumentValue
{
    public string UnserializableValue { get; set; }

    public string ObjectId { get; set; }

    public JsonElement? Value { get; set; }
}
