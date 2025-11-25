// <copyright file="EnumHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PuppeteerSharp.Helpers.Json;

internal static class EnumHelper
{
    public static string[] GetNames<TEnum>()
        where TEnum : struct, Enum =>
#if NET8_0_OR_GREATER
        Enum.GetNames<TEnum>();
#else
        Enum.GetNames(typeof(TEnum));
#endif

    public static TEnum[] GetValues<TEnum>()
        where TEnum : struct, Enum =>
#if NET8_0_OR_GREATER
        Enum.GetValues<TEnum>();
#else
        (TEnum[])Enum.GetValues(typeof(TEnum));
#endif

    public static TEnum Parse<TEnum>(string value, bool ignoreCase)
        where TEnum : struct, Enum =>
#if NET8_0_OR_GREATER
        Enum.Parse<TEnum>(value, ignoreCase);
#else
        (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
#endif

    public static TEnum ToEnum<TEnum>(this string value)
        where TEnum : struct, Enum
        => StringToEnum<TEnum>.Cache[value];

    public static string ToValueString<TEnum>(this TEnum value)
        where TEnum : struct, Enum
        => EnumToString<TEnum>.Cache[value];

    public static TEnum FromValueString<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (StringToEnum<TEnum>.Cache.TryGetValue(value, out var enumValue))
        {
            return enumValue;
        }

        var defaultEnumAttribute = typeof(TEnum).GetCustomAttribute<DefaultEnumValueAttribute>();
        if (defaultEnumAttribute != null)
        {
            return (TEnum)(object)defaultEnumAttribute.Value;
        }

        throw new ArgumentException($"Unknown value '{value}' for enum {typeof(TEnum).Name}");
    }

    private static class StringToEnum<TEnum>
        where TEnum : struct, Enum
    {
        public static readonly IReadOnlyDictionary<string, TEnum> Cache = Compute();

        private static Dictionary<string, TEnum> Compute()
        {
            var names = GetNames<TEnum>();
            var dictionary = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                var field = typeof(TEnum).GetField(name);
                var fieldValue = (TEnum)field.GetValue(null);
                dictionary[name] = fieldValue;
                if (field.GetCustomAttribute<EnumMemberAttribute>()?.Value is { } enumMember)
                {
                    dictionary[enumMember] = fieldValue;
                }
            }

            return dictionary;
        }
    }

    private static class EnumToString<TEnum>
        where TEnum : struct, Enum
    {
        public static readonly IReadOnlyDictionary<TEnum, string> Cache = Compute();

        private static Dictionary<TEnum, string> Compute()
        {
            var names = GetNames<TEnum>();
            var dictionary = new Dictionary<TEnum, string>();
            foreach (var name in names)
            {
                var field = typeof(TEnum).GetField(name);
                var valueName = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
                var fieldValue = (TEnum)field.GetValue(null);
                dictionary[fieldValue] = valueName;
            }

            return dictionary;
        }
    }
}
