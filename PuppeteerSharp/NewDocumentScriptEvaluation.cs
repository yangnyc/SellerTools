// <copyright file="NewDocumentScriptEvaluation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

/// <summary>
/// New document information.
/// </summary>
public class NewDocumentScriptEvaluation(string documentIdentifierIdentifier)
{
    /// <summary>
    /// Gets or sets new document identifier.
    /// </summary>
    public string Identifier { get; set; } = documentIdentifierIdentifier;
}
