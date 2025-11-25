// <copyright file="PageHandleJavaScriptDialogRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class PageHandleJavaScriptDialogRequest
    {
        public bool Accept { get; set; }

        public string PromptText { get; set; }
    }
}
