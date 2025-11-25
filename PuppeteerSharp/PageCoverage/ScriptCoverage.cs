// <copyright file="ScriptCoverage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage;

/// <summary>
/// Coverage data for a JavaScript script.
/// </summary>
public record ScriptCoverage
{
    /// <summary>
    /// Gets or sets javaScript script id.
    /// </summary>
    public string ScriptId { get; set; }

    /// <summary>
    /// Gets or sets javaScript script name or url.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets functions contained in the script that has coverage data.
    /// </summary>
    public FunctionCoverage[] Functions { get; set; }
}
