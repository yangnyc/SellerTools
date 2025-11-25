// <copyright file="Request.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging;

using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

internal class Request
{
    public HttpMethod Method { get; set; }

    [JsonConverter(typeof(LowSurrogateConverter))]
    public string PostData { get; set; }

    public Dictionary<string, string> Headers { get; set; } = [];

    public string Url { get; set; }

    public bool? HasPostData { get; set; }
}
