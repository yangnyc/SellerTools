// <copyright file="CSSStyleSheetAddedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class CSSStyleSheetAddedResponse
    {
        public CSSStyleSheetAddedResponseHeader Header { get; set; }

        public class CSSStyleSheetAddedResponseHeader
        {
            public string StyleSheetId { get; set; }

            public string SourceURL { get; set; }
        }
    }
}
