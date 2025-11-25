// <copyright file="DeviceRequestPromptManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;
using PuppeteerSharp.Helpers.Json;

/// <summary>
/// Prompt manager.
/// </summary>
public class DeviceRequestPromptManager
{
    private readonly TimeoutSettings timeoutSettings;
    private ICDPSession client;
    private TaskCompletionSource<DeviceRequestPrompt> deviceRequestPromptTcs;

    internal DeviceRequestPromptManager(ICDPSession client, TimeoutSettings timeoutSettings)
    {
        this.client = client;
        this.timeoutSettings = timeoutSettings;
        this.client.MessageReceived += this.OnMessageReceived;
    }

    internal async Task<DeviceRequestPrompt> WaitForDevicePromptAsync(WaitForOptions options = default)
    {
        if (this.client == null)
        {
            throw new PuppeteerException("Cannot wait for device prompt through detached session!");
        }

        var needsEnable = this.deviceRequestPromptTcs == null || this.deviceRequestPromptTcs.Task.IsCompleted;
        var enableTask = Task.CompletedTask;

        if (needsEnable)
        {
            this.deviceRequestPromptTcs = new TaskCompletionSource<DeviceRequestPrompt>();
            enableTask = this.client.SendAsync("DeviceAccess.enable");
        }

        var timeout = options?.Timeout ?? this.timeoutSettings.Timeout;
        var task = this.deviceRequestPromptTcs.Task.WithTimeout(timeout);

        await Task.WhenAll(enableTask, task).ConfigureAwait(false);

        return await this.deviceRequestPromptTcs.Task.ConfigureAwait(false);
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            switch (e.MessageID)
            {
                case "DeviceAccess.deviceRequestPrompted":
                    this.OnDeviceRequestPrompted(e.MessageData.ToObject<DeviceAccessDeviceRequestPromptedResponse>());
                    break;
                case "Target.detachedFromTarget":
                    this.client = null;
                    break;
            }
        }
        catch (Exception ex)
        {
            var message = $"Connection failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
            (this.client as CDPSession)?.Close(message);
        }
    }

    private void OnDeviceRequestPrompted(DeviceAccessDeviceRequestPromptedResponse e)
    {
        if (this.deviceRequestPromptTcs == null)
        {
            return;
        }

        if (this.client == null)
        {
            this.deviceRequestPromptTcs.TrySetException(new PuppeteerException("Session closed. Most likely the target has been closed."));
            return;
        }

        var devicePrompt = new DeviceRequestPrompt(this.client, this.timeoutSettings, e);
        this.deviceRequestPromptTcs.TrySetResult(devicePrompt);
    }
}
