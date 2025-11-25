// <copyright file="FunctionCoverage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage;

/// <summary>
/// Coverage data for a JavaScript function.
/// </summary>
public record FunctionCoverage
{
    /// <summary>
    /// Gets or sets javaScript function name.
    /// </summary>
    public string FunctionName { get; set; }

    /// <summary>
    /// Gets or sets source ranges inside the function with coverage data.
    /// </summary>
    public CoverageRange[] Ranges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether whether coverage data for this function has block granularity.
    /// </summary>
    public bool IsBlockCoverage { get; set; }
}
