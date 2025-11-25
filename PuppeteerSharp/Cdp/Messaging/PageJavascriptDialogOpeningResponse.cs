// <copyright file="PageJavascriptDialogOpeningResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class PageJavascriptDialogOpeningResponse
    {
        public DialogType Type { get; set; }

        public string DefaultPrompt { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
