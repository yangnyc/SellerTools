// <copyright file="NavigatedWithinDocumentResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class NavigatedWithinDocumentResponse
    {
        public string FrameId { get; set; }

        public string Url { get; set; }
    }
}
