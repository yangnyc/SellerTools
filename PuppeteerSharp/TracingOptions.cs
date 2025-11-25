// <copyright file="TracingOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Collections.Generic;

    /// <summary>
    /// Tracing options used on <see cref="ITracing.StartAsync(TracingOptions)"/>.
    /// </summary>
    public class TracingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether Tracing should capture screenshots in the trace.
        /// </summary>
        /// <value>Screenshots option.</value>
        public bool Screenshots { get; set; }

        /// <summary>
        /// Gets or sets a path to write the trace file to.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets specify custom categories to use instead of default.
        /// </summary>
        /// <value>The categories.</value>
        public List<string> Categories { get; set; }
    }
}
