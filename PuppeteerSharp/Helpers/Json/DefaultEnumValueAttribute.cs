// <copyright file="DefaultEnumValueAttribute.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers.Json;

using System;

[AttributeUsage(AttributeTargets.Enum)]
internal sealed class DefaultEnumValueAttribute(int value) : Attribute
{
    public int Value { get; } = value;
}
