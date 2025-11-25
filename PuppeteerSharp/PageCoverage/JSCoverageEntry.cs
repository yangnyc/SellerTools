// <copyright file="JSCoverageEntry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage;

/// <summary>
/// The CoverageEntry class for JavaScript.
/// </summary>
public record JSCoverageEntry : CoverageEntry
{
    /// <summary>
    /// Gets or sets raw V8 script coverage entry.
    /// </summary>
    public ScriptCoverage RawScriptCoverage { get; set; }
}
