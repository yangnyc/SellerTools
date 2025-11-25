// <copyright file="CreateBrowserContextResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    internal class CreateBrowserContextResponse
    {
        [JsonConverter(typeof(AnyTypeToStringConverter))]
        public string BrowserContextId { get; set; }
    }
}
