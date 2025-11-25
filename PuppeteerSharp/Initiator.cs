// <copyright file="Initiator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

/// <summary>
/// Information about the request initiator.
/// </summary>
public class Initiator
{
    /// <summary>
    /// Gets or sets the type of the initiator.
    /// </summary>
    public InitiatorType Type { get; set; }

    /// <summary>
    /// Gets or sets initiator URL, set for Parser type or for Script type (when script is importing module) or for SignedExchange type.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets initiator line number, set for Parser type or for Script type (when script is importing module) (0-based).
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets initiator column number, set for Parser type or for Script type (when script is importing module) (0-based).
    /// </summary>
    public int? ColumnNumber { get; set; }

    /// <summary>
    /// Gets or sets set if another request triggered this request (e.g. preflight).
    /// </summary>
    public string RequestId { get; set; }
}
