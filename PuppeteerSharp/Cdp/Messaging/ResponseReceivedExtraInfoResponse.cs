// <copyright file="ResponseReceivedExtraInfoResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;
    using System.Net;

    internal class ResponseReceivedExtraInfoResponse
    {
        public string RequestId { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string HeadersText { get; set; }
    }
}
