// <copyright file="ConnectionResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json;

    internal class ConnectionResponse
    {
        public int? Id { get; set; }

        public ConnectionError Error { get; set; }

        public JsonElement? Result { get; set; }

        public string Method { get; set; }

        public JsonElement? Params { get; set; }

        public string SessionId { get; set; }
    }
}
