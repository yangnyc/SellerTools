// <copyright file="MessageTask.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json;
    using System.Threading.Tasks;

    internal class MessageTask
    {
        internal byte[] Message { get; set; }

        internal TaskCompletionSource<JsonElement?> TaskWrapper { get; set; }

        internal string Method { get; set; }
    }
}
