// <copyright file="CdpCDPSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;

// This is a pretty terrible name, but it matches upstream

/// <inheritdoc />
public class CdpCDPSession : CDPSession
{
    private readonly ConcurrentDictionary<int, MessageTask> callbacks = new();
    private readonly string parentSessionId;
    private readonly TargetType targetType;
    private int lastId;

    internal CdpCDPSession(Connection connection, TargetType targetType, string sessionId, string parentSessionId)
    {
        this.Connection = connection;
        this.targetType = targetType;
        this.Id = sessionId;
        this.parentSessionId = parentSessionId;
    }

    internal override CDPSession ParentSession
        => string.IsNullOrEmpty(this.parentSessionId) ? this : this.Connection.GetSession(this.parentSessionId) ?? this;

    internal bool IsClosed { get; private set; }

    /// <inheritdoc />
    public override Task DetachAsync()
    {
        if (this.Connection == null)
        {
            throw new PuppeteerException($"Session already detached.Most likely the {this.targetType} has been closed.");
        }

        return this.Connection.SendAsync("Target.detachFromTarget", new TargetDetachFromTargetRequest { SessionId = this.Id });
    }

    /// <inheritdoc />
    public override async Task<JsonElement?> SendAsync(
        string method,
        object args = null,
        bool waitForCallback = true,
        CommandOptions options = null)
    {
        if (this.Connection == null)
        {
            throw new TargetClosedException(
                $"Protocol error ({method}): Session closed. " +
                $"Most likely the {this.targetType} has been closed." +
                $"Close reason: {this.CloseReason}",
                this.CloseReason);
        }

        var id = this.GetMessageId();
        var message = this.Connection.GetMessage(id, method, args, this.Id);

        MessageTask callback = null;
        if (waitForCallback)
        {
            callback = new MessageTask
            {
                TaskWrapper =
                    new TaskCompletionSource<JsonElement?>(TaskCreationOptions.RunContinuationsAsynchronously),
                Method = method,
                Message = message,
            };
            this.callbacks[id] = callback;
        }

        try
        {
            await this.Connection.RawSendAsync(message, options).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (waitForCallback && this.callbacks.TryRemove(id, out _))
            {
                callback.TaskWrapper.TrySetException(new MessageException(ex.Message, ex));
            }
        }

        return waitForCallback
            ? await callback.TaskWrapper.Task.WithTimeout(options?.Timeout ?? this.Connection.ProtocolTimeout)
                .ConfigureAwait(false)
            : null;
    }

    internal bool HasPendingCallbacks() => !this.callbacks.IsEmpty;

    internal int GetMessageId() => Interlocked.Increment(ref this.lastId);

    internal IEnumerable<MessageTask> GetPendingMessages() => this.callbacks.Values;

    internal void OnMessage(ConnectionResponse obj)
    {
        var id = obj.Id;

        if (id.HasValue && this.callbacks.TryRemove(id.Value, out var callback))
        {
            this.Connection.MessageQueue.Enqueue(callback, obj);
        }
        else if (obj.Method is not null && obj.Params is not null)
        {
            // If the message was set to not wait for callback, the message won't be in the callbacks dictionary
            // And it might have no method set.
            var method = obj.Method;
            this.OnMessageReceived(new MessageEventArgs { MessageID = method, MessageData = (JsonElement)obj.Params });
        }
    }

    internal override void Close(string closeReason)
    {
        if (this.IsClosed)
        {
            return;
        }

        this.CloseReason = closeReason;
        this.IsClosed = true;

        foreach (var callback in this.callbacks.Values)
        {
            callback.TaskWrapper.TrySetException(new TargetClosedException(
                $"Protocol error({callback.Method}): Target closed.",
                closeReason));
        }

        this.callbacks.Clear();
        this.OnDisconnected();
        this.Connection = null;
    }
}
