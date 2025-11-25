// <copyright file="AccessibilitySnapshotOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageAccessibility
{
    /// <summary>
    /// <see cref="IAccessibility.SnapshotAsync(AccessibilitySnapshotOptions)"/>.
    /// </summary>
    /// <seealso cref="IPage.Accessibility"/>
    public class AccessibilitySnapshotOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether prune uninteresting nodes from the tree. Defaults to true.
        /// </summary>
        public bool InterestingOnly { get; set; } = true;

        /// <summary>
        /// Gets or sets the root DOM element for the snapshot. Defaults to the whole page.
        /// </summary>
        public IElementHandle Root { get; set; }
    }
}
