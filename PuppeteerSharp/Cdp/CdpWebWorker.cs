// <copyright file="CdpWebWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers.Json;

/// <inheritdoc />
public class CdpWebWorker : WebWorker
{
    private readonly ILogger logger;
    private readonly Func<ConsoleType, IJSHandle[], StackTrace, Task> consoleAPICalled;
    private readonly Action<EvaluateExceptionResponseDetails> exceptionThrown;
    private readonly string id;
    private readonly TargetType targetType;

    internal CdpWebWorker(
        CDPSession client,
        string url,
        string targetId,
        TargetType targetType,
        Func<ConsoleType, IJSHandle[], StackTrace, Task> consoleAPICalled,
        Action<EvaluateExceptionResponseDetails> exceptionThrown)
        : base(url)
    {
        this.logger = client.Connection.LoggerFactory.CreateLogger<WebWorker>();
        this.id = targetId;
        this.Client = client;
        this.targetType = targetType;
        this.World = new IsolatedWorld(null, this, new TimeoutSettings(), true);
        this.consoleAPICalled = consoleAPICalled;
        this.exceptionThrown = exceptionThrown;
        client.MessageReceived += this.OnMessageReceived;

        _ = client.SendAsync("Runtime.enable").ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                {
                    this.logger.LogError(task.Exception!.Message);
                }
            },
            TaskScheduler.Default);

        _ = client.SendAsync("Log.enable").ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                {
                    this.logger.LogError(task.Exception!.Message);
                }
            },
            TaskScheduler.Default);
    }

    /// <inheritdoc/>
    public override CDPSession Client { get; }

    internal override IsolatedWorld World { get; }

    /// <summary>
    /// Closes the worker.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the worker is closed.</returns>
    public override async Task CloseAsync()
    {
        switch (this.targetType)
        {
            case TargetType.ServiceWorker:
            case TargetType.SharedWorker:
                // For service and shared workers we need to close the target and detach to allow
                // the worker to stop.
                await this.Client.Connection.SendAsync(
                    "Target.closeTarget",
                    new TargetCloseTargetRequest()
                    {
                        TargetId = this.id,
                    }).ConfigureAwait(false);

                await this.Client.Connection.SendAsync(
                    "Target.detachFromTarget",
                    new TargetDetachFromTargetRequest()
                    {
                        SessionId = this.Client.Id,
                    }).ConfigureAwait(false);
                break;
            default:
                await this.EvaluateFunctionAsync(@"() => {
                        self.close();
                    }").ConfigureAwait(false);
                break;
        }
    }

    private async void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            switch (e.MessageID)
            {
                case "Runtime.executionContextCreated":
                    this.OnExecutionContextCreated(e.MessageData.ToObject<RuntimeExecutionContextCreatedResponse>());
                    break;
                case "Runtime.consoleAPICalled":
                    await this.OnConsoleAPICalledAsync(e).ConfigureAwait(false);
                    break;
                case "Runtime.exceptionThrown":
                    this.OnExceptionThrown(e.MessageData.ToObject<RuntimeExceptionThrownResponse>());
                    break;
            }
        }
        catch (Exception ex)
        {
            var message = $"Worker failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
            this.logger.LogError(ex, message);
            this.Client.Close(message);
        }
    }

    private void OnExceptionThrown(RuntimeExceptionThrownResponse e) => this.exceptionThrown(e.ExceptionDetails);

    private async Task OnConsoleAPICalledAsync(MessageEventArgs e)
    {
        var consoleData = e.MessageData.ToObject<PageConsoleResponse>();
        await this.consoleAPICalled(
            consoleData.Type,
            consoleData.Args.Select(i => new CdpJSHandle(this.World, i)).ToArray(),
            consoleData.StackTrace)
                .ConfigureAwait(false);
    }

    private void OnExecutionContextCreated(RuntimeExecutionContextCreatedResponse e)
    {
        if (!this.World.HasContext)
        {
            this.World.SetNewContext(this.Client, e.Context, this.World);
        }
    }
}
