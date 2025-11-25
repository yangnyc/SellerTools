// <copyright file="ProfilerTakePreciseCoverageResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using PuppeteerSharp.PageCoverage;

    internal class ProfilerTakePreciseCoverageResponse
    {
        public ScriptCoverage[] Result { get; set; }
    }
}
