// <copyright file="EvaluateExceptionResponseInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    internal class EvaluateExceptionResponseInfo
    {
        public string Description { get; set; }

        [JsonConverter(typeof(AnyTypeToStringConverter))]
        public string Value { get; set; }
    }
}
