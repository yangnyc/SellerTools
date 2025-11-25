// <copyright file="WaitForSelectorOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="IPage.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
    /// <seealso cref="IFrame.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
    public class WaitForSelectorOptions : WaitForOptions
    {
        /// <summary>
        /// Gets or sets wait for element to be present in DOM and to be visible.
        /// </summary>
        public bool? Visible { get; set; }

        /// <summary>
        /// Gets or sets wait for element to not be found in the DOM or to be hidden.
        /// </summary>
        public bool? Hidden { get; set; }

        /// <summary>
        /// Gets or sets root element.
        /// </summary>
        public IElementHandle Root { get; set; }
    }
}
