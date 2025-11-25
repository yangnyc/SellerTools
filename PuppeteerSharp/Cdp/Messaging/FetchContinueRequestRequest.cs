// <copyright file="FetchContinueRequestRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class FetchContinueRequestRequest
    {
        public string RequestId { get; set; }

        public string Url { get; set; }

        public string Method { get; set; }

        public string PostData { get; set; }

        public Header[] Headers { get; set; }
    }
}
