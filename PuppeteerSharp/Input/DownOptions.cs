// <copyright file="DownOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Input
{
    /// <summary>
    /// options to use with <see cref="IKeyboard.DownAsync(string, DownOptions)"/>.
    /// </summary>
    public class DownOptions
    {
        /// <summary>
        /// Gets or sets if specified, generates an input event with this text.
        /// </summary>
        public string Text { get; set; }
    }
}
