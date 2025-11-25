// <copyright file="WaitForFunctionOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="IPage.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="IFrame.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="WaitForSelectorOptions"/>
    public class WaitForFunctionOptions : WaitForOptions
    {
        /// <summary>
        /// Gets or sets an interval at which the <c>pageFunction</c> is executed. defaults to <see cref="WaitForFunctionPollingOption.Raf"/>.
        /// </summary>
        public WaitForFunctionPollingOption Polling { get; set; } = WaitForFunctionPollingOption.Raf;

        /// <summary>
        /// Gets or sets an interval at which the <c>pageFunction</c> is executed. If no value is specified will use <see cref="Polling"/>.
        /// </summary>
        public int? PollingInterval { get; set; }

        /// <summary>
        /// Gets or sets root element.
        /// </summary>
        internal IElementHandle Root { get; set; }
    }
}
