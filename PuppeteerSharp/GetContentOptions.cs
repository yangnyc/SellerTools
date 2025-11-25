// <copyright file="GetContentOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

/// <summary>
/// Options for <see cref="IPage.GetContentAsync(GetContentOptions)"/> and <see cref="IFrame.GetContentAsync(GetContentOptions)"/>.
/// </summary>
public class GetContentOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether replace lone surrogates with U+FFFD replacement character.
    /// </summary>
    /// <remarks>
    /// This functionality relies on the toWellFormed function. <a href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/toWellFormed">See.</a>.
    /// It's set to false by default to prevent extra processing when not needed.
    /// </remarks>
    public bool ReplaceLoneSurrogates { get; set; } = false;
}
