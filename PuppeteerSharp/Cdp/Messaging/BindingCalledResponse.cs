// <copyright file="BindingCalledResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Text.Json;
    using PuppeteerSharp.Helpers.Json;

    internal class BindingCalledResponse
    {
        private string payloadJson;

        public int ExecutionContextId { get; set; }

        public BindingCalledResponsePayload BindingPayload { get; set; }

        public string Payload
        {
            get => this.payloadJson;
            set
            {
                this.payloadJson = value;
                var json = JsonSerializer.Deserialize<JsonElement>(this.payloadJson, JsonHelper.DefaultJsonSerializerSettings.Value);
                this.BindingPayload = json.ToObject<BindingCalledResponsePayload>();
                this.BindingPayload.JsonObject = json;
            }
        }

        internal class BindingCalledResponsePayload
        {
            public string Type { get; set; }

            public string Name { get; set; }

            public JsonElement[] Args { get; set; }

            public int Seq { get; set; }

            public JsonElement JsonObject { get; set; }

            public bool IsTrivial { get; set; }
        }
    }
}
