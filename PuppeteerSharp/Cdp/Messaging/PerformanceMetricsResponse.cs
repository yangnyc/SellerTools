// <copyright file="PerformanceMetricsResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class PerformanceMetricsResponse
    {
        public string Title { get; set; }

        public List<Metric> Metrics { get; set; }
    }
}
