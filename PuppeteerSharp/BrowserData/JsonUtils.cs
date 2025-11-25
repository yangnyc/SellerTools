// <copyright file="JsonUtils.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.BrowserData
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers.Json;

    internal static class JsonUtils
    {
        public static async Task<T> GetAsync<T>(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(url).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(response, JsonHelper.DefaultJsonSerializerSettings.Value);
        }

        internal static async Task<string> GetTextAsync(string url)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(url).ConfigureAwait(false);
        }
    }
}
