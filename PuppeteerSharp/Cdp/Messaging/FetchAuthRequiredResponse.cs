// <copyright file="FetchAuthRequiredResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;
    using System.Net;

    internal class FetchAuthRequiredResponse
    {
        public string RequestId { get; set; }

        public Payload Request { get; set; }

        public string FrameId { get; set; }

        public ResourceType ResourceType { get; set; }

        public bool IsNavigationRequest { get; set; }

        public Dictionary<string, object> ResponseHeaders { get; set; }

        public HttpStatusCode ResponseStatusCode { get; set; }

        public string RedirectUrl { get; set; }

        public AuthChallengeData AuthChallenge { get; set; }

        internal class AuthChallengeData
        {
            public string Source { get; set; }

            public string Origin { get; set; }

            public string Scheme { get; set; }

            public string Realm { get; set; }
        }
    }
}
