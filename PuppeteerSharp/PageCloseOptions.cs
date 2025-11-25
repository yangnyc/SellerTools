// <copyright file="PageCloseOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Page close options.
    /// </summary>
    /// <seealso cref="IPage.CloseAsync(PageCloseOptions)"/>
    public class PageCloseOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether defaults to <c>false</c>. Whether to run the beforeunload page handlers.
        /// </summary>
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/Events/beforeunload"/>
        public bool RunBeforeUnload { get; set; }
    }
}
