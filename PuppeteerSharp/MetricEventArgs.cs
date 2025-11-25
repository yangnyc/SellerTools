// <copyright file="MetricEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <seealso cref="IPage.Metrics"/> arguments.
    /// </summary>
    public class MetricEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricEventArgs"/> class.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="metrics">Metrics.</param>
        public MetricEventArgs(string title, Dictionary<string, decimal> metrics)
        {
            this.Title = title;
            this.Metrics = metrics;
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public Dictionary<string, decimal> Metrics { get; }
    }
}
