// <copyright file="StringExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;

    internal static class StringExtensions
    {
        /// <summary>
        /// Quotes the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The string to quote.</param>
        /// <returns>A quoted string.</returns>
        public static string Quote(this string value)
        {
            if (!IsQuoted(value))
            {
                value = string.Concat("\"", value, "\"");
            }

            return value;
        }

        /// <summary>
        /// Unquotes the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The string to unquote.</param>
        /// <returns>An unquoted string.</returns>
        public static string Unquote(this string value)
            => IsQuoted(value) ? value.Substring(1, value.Length - 2) : value;

        private static bool IsQuoted(this string value)
        {
            return value.StartsWith("\"", StringComparison.OrdinalIgnoreCase)
                   && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase);
        }
    }
}
