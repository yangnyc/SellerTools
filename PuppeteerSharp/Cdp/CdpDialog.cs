// <copyright file="CdpDialog.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;

/// <inheritdoc />
public class CdpDialog : Dialog
{
    private readonly CDPSession client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CdpDialog"/> class.
    /// </summary>
    /// <param name="client">Client.</param>
    /// <param name="type">Type.</param>
    /// <param name="message">Message.</param>
    /// <param name="defaultValue">Default value.</param>
    public CdpDialog(CDPSession client, DialogType type, string message, string defaultValue)
        : base(type, message, defaultValue)
    {
        this.client = client;
    }

    internal override Task HandleAsync(bool accept, string text)
        => this.client.SendAsync("Page.handleJavaScriptDialog", new PageHandleJavaScriptDialogRequest
        {
            Accept = accept,
            PromptText = text,
        });
}
