// <copyright file="IBrowserOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Browser options.
    /// </summary>
    public interface IBrowserOptions
    {
        /// <summary>
        /// Gets a value indicating whether whether to ignore HTTPS errors during navigation. Defaults to false.
        /// </summary>
        bool AcceptInsecureCerts { get; }

        /// <summary>
        /// Gets or sets the default Viewport.
        /// </summary>
        /// <value>The default Viewport.</value>
        ViewPortOptions DefaultViewport { get; set; }
    }
}
