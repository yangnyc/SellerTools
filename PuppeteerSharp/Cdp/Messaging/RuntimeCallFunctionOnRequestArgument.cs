// <copyright file="RuntimeCallFunctionOnRequestArgument.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json.Serialization;

    internal class RuntimeCallFunctionOnRequestArgument
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public object Value { get; set; }
    }
}
