// <copyright file="PerformanceGetMetricsResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class PerformanceGetMetricsResponse
    {
        public List<Metric> Metrics { get; set; }
    }
}
