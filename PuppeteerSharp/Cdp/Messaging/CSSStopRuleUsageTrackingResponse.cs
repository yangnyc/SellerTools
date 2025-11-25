// <copyright file="CSSStopRuleUsageTrackingResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class CSSStopRuleUsageTrackingResponse
    {
        public CSSStopRuleUsageTrackingRuleUsage[] RuleUsage { get; set; }

        internal class CSSStopRuleUsageTrackingRuleUsage
        {
            public string StyleSheetId { get; set; }

            public int StartOffset { get; set; }

            public int EndOffset { get; set; }

            public bool Used { get; set; }
        }
    }
}
