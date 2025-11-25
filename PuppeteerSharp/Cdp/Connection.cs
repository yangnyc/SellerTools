// <copyright file="Connection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;
    using PuppeteerSharp.QueryHandlers;
    using PuppeteerSharp.Transport;

    /// <summary>
    /// A connection handles the communication with a Chromium browser.
    /// </summary>
    public sealed class Connection : IDisposable, ICDPConnection
    {
        internal const int DefaultCommandTimeout = 180_000;
        private readonly ILogger logger;
        private readonly TaskQueue callbackQueue = new();

        private readonly ConcurrentDictionary<int, MessageTask> callbacks = new();
        private readonly AsyncDictionaryHelper<string, CdpCDPSession> sessions = new("Session {0} not found");
        private readonly List<string> manuallyAttached = [];
        private int lastId;

        private Connection(string url, int delay, bool enqueueAsyncMessages, IConnectionTransport transport, ILoggerFactory loggerFactory = null, int protocolTimeout = DefaultCommandTimeout)
        {
            this.LoggerFactory = loggerFactory ?? new LoggerFactory();
            this.Url = url;
            this.Delay = delay;
            this.Transport = transport;

            this.logger = this.LoggerFactory.CreateLogger<Connection>();
            this.ProtocolTimeout = protocolTimeout;
            this.MessageQueue = new AsyncMessageQueue(enqueueAsyncMessages, this.logger);

            this.Transport.MessageReceived += this.Transport_MessageReceived;
            this.Transport.Closed += this.Transport_Closed;
        }

        /// <summary>
        /// Occurs when the connection is closed.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Occurs when a message from chromium is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        internal event EventHandler<SessionEventArgs> SessionAttached;

        internal event EventHandler<SessionEventArgs> SessionDetached;

        /// <summary>
        /// Gets the WebSocket URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; }

        /// <summary>
        /// Gets the sleep time when a message is received.
        /// </summary>
        /// <value>The delay.</value>
        public int Delay { get; }

        /// <summary>
        /// Gets the Connection transport.
        /// </summary>
        /// <value>Connection transport.</value>
        public IConnectionTransport Transport { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Connection"/> is closed.
        /// </summary>
        /// <value><c>true</c> if is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed { get; internal set; }

        /// <summary>
        /// Gets connection close reason.
        /// </summary>
        public string CloseReason { get; private set; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        /// <value>The logger factory.</value>
        public ILoggerFactory LoggerFactory { get; }

        internal AsyncMessageQueue MessageQueue { get; }

        // The connection is a good place to keep the state of custom queries and injectors.
        // Although I consider that the Browser class would be a better place for this,
        // The connection is being shared between all the components involved in one browser instance
        internal CustomQuerySelectorRegistry CustomQuerySelectorRegistry { get; } = new();

        internal ScriptInjector ScriptInjector { get; } = new();

        internal int ProtocolTimeout { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task<JsonElement?> SendAsync(string method, object args = null, bool waitForCallback = true, CommandOptions options = null)
        {
            if (this.IsClosed)
            {
                throw new TargetClosedException($"Protocol error({method}): Target closed.", this.CloseReason);
            }

            var id = this.GetMessageId();
            var message = this.GetMessage(id, method, args);

            MessageTask callback = null;
            if (waitForCallback)
            {
                callback = new MessageTask
                {
                    TaskWrapper = new TaskCompletionSource<JsonElement?>(TaskCreationOptions.RunContinuationsAsynchronously),
                    Method = method,
                    Message = message,
                };
                this.callbacks[id] = callback;
            }

            await this.RawSendAsync(message, options).ConfigureAwait(false);
            return waitForCallback ? await callback.TaskWrapper.Task.WithTimeout(this.ProtocolTimeout).ConfigureAwait(false) : null;
        }

        /// <inheritdoc/>
        public async Task<T> SendAsync<T>(string method, object args = null, CommandOptions options = null)
        {
            var response = await this.SendAsync(method, args, true, options).ConfigureAwait(false);
            return response!.Value.ToObject<T>();
        }

        internal static async Task<Connection> Create(string url, IConnectionOptions connectionOptions, ILoggerFactory loggerFactory = null, CancellationToken cancellationToken = default)
        {
            var transportFactory = connectionOptions.TransportFactory ?? WebSocketTransport.DefaultTransportFactory;
            var transport = await transportFactory(new Uri(url), connectionOptions, cancellationToken).ConfigureAwait(false);

            return new Connection(url, connectionOptions.SlowMo, connectionOptions.EnqueueAsyncMessages, transport, loggerFactory, connectionOptions.ProtocolTimeout);
        }

        internal static Connection FromSession(CdpCDPSession session) => session.Connection;

        internal int GetMessageId() => Interlocked.Increment(ref this.lastId);

        internal Task RawSendAsync(byte[] message, CommandOptions options = null)
        {
            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Send ► {Message}", Encoding.UTF8.GetString(message));
            }

            return this.Transport.SendAsync(message);
        }

        internal byte[] GetMessage(int id, string method, object args, string sessionId = null)
            => JsonSerializer.SerializeToUtf8Bytes(
                new ConnectionRequest { Id = id, Method = method, Params = args, SessionId = sessionId },
                JsonHelper.DefaultJsonSerializerSettings.Value);

        internal bool IsAutoAttached(string targetId)
            => !this.manuallyAttached.Contains(targetId);

        internal async Task<CDPSession> CreateSessionAsync(TargetInfo targetInfo, bool isAutoAttachEmulated)
        {
            if (!isAutoAttachEmulated)
            {
                this.manuallyAttached.Add(targetInfo.TargetId);
            }

            var sessionId = (await this.SendAsync<TargetAttachToTargetResponse>(
                "Target.attachToTarget",
                new TargetAttachToTargetRequest
                {
                    TargetId = targetInfo.TargetId,
                    Flatten = true,
                }).ConfigureAwait(false)).SessionId;
            this.manuallyAttached.Remove(targetInfo.TargetId);
            return await this.GetSessionAsync(sessionId).ConfigureAwait(false);
        }

        internal bool HasPendingCallbacks() => !this.callbacks.IsEmpty;

        internal void Close(string closeReason)
        {
            if (this.IsClosed)
            {
                return;
            }

            this.IsClosed = true;
            this.CloseReason = closeReason;

            this.Transport.StopReading();
            this.Disconnected?.Invoke(this, EventArgs.Empty);

            foreach (var session in this.sessions.Values)
            {
                session.Close(closeReason);
            }

            this.sessions.Clear();

            foreach (var response in this.callbacks.Values)
            {
                response.TaskWrapper.TrySetException(new TargetClosedException(
                    $"Protocol error({response.Method}): Target closed.",
                    closeReason));
            }

            this.callbacks.Clear();
            this.MessageQueue.Dispose();
        }

        internal CdpCDPSession GetSession(string sessionId) => this.sessions.GetValueOrDefault(sessionId);

        /// <summary>
        /// Releases all resource used by the <see cref="Connection"/> object.
        /// It will raise the <see cref="Disconnected"/> event and dispose <see cref="Transport"/>.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="Connection"/>. The
        /// <see cref="Dispose()"/> method leaves the <see cref="Connection"/> in an unusable state.
        /// After calling <see cref="Dispose()"/>, you must release all references to the
        /// <see cref="Connection"/> so the garbage collector can reclaim the memory that the
        /// <see cref="Connection"/> was occupying.</remarks>
        /// <param name="disposing">Indicates whether disposal was initiated by <see cref="Dispose()"/> operation.</param>
        private void Dispose(bool disposing)
        {
            this.Close("Connection disposed");
            this.Transport.MessageReceived -= this.Transport_MessageReceived;
            this.Transport.Closed -= this.Transport_Closed;
            this.Transport.Dispose();
            this.callbackQueue.Dispose();
        }

        private Task<CdpCDPSession> GetSessionAsync(string sessionId) => this.sessions.GetItemAsync(sessionId);

        private async void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                await this.callbackQueue.Enqueue(() => this.ProcessMessage(e)).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // We could just catch ObjectDisposedException but as this is an event listener
                // we don't want to crash the whole process.
                this.logger.LogError(exception, $"Failed to process message {e.Message}");
            }
        }

        private async Task ProcessMessage(MessageReceivedEventArgs e)
        {
            try
            {
                var response = e.Message;
                ConnectionResponse obj = null;

                if (response.Length > 0 && this.Delay > 0)
                {
                    await Task.Delay(this.Delay).ConfigureAwait(false);
                }

                try
                {
                    obj = JsonSerializer.Deserialize<ConnectionResponse>(response, JsonHelper.DefaultJsonSerializerSettings.Value);
                }
                catch (JsonException exc)
                {
                    this.logger.LogError(exc, "Failed to deserialize response");
                    return;
                }

                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    this.logger.LogTrace("◀ Receive {Message}", Encoding.UTF8.GetString(response));
                }

                this.ProcessIncomingMessage(obj);
            }
            catch (Exception ex)
            {
                var message = $"Connection failed to process {e.Message}. {ex.Message}. {ex.StackTrace}";
                this.logger.LogError(ex, message);
                this.Close(message);
            }
        }

        private void ProcessIncomingMessage(ConnectionResponse obj)
        {
            var method = obj.Method;
            var param = obj.Params?.ToObject<ConnectionResponseParams>();

            if (method == "Target.attachedToTarget")
            {
                var sessionId = param.SessionId;
                var session = new CdpCDPSession(this, param.TargetInfo.Type, sessionId, obj.SessionId);
                this.sessions.AddItem(sessionId, session);

                this.SessionAttached?.Invoke(this, new SessionEventArgs(session));

                if (obj.SessionId != null && this.sessions.TryGetValue(obj.SessionId, out var parentSession))
                {
                    parentSession.OnSessionAttached(session);
                }
            }
            else if (method == "Target.detachedFromTarget")
            {
                var sessionId = param.SessionId;
                if (this.sessions.TryRemove(sessionId, out var session) && !session.IsClosed)
                {
                    session.Close("Target.detachedFromTarget");
                    this.SessionDetached?.Invoke(this, new SessionEventArgs(session));

                    if (this.sessions.TryGetValue(sessionId, out var parentSession))
                    {
                        parentSession.OnSessionDetached(session);
                    }
                }
            }

            if (!string.IsNullOrEmpty(obj.SessionId))
            {
                var session = this.GetSession(obj.SessionId);
                session?.OnMessage(obj);
            }
            else if (obj.Id.HasValue)
            {
                // If we get the object we are waiting for we return if
                // if not we add this to the list, sooner or later some one will come for it
                if (this.callbacks.TryRemove(obj.Id.Value, out var callback))
                {
                    this.MessageQueue.Enqueue(callback, obj);
                }
            }
            else
            {
                this.MessageReceived?.Invoke(this, new MessageEventArgs
                {
                    MessageID = method,
                    MessageData = (JsonElement)obj.Params,
                });
            }
        }

        private void Transport_Closed(object sender, TransportClosedEventArgs e) => this.Close(e.CloseReason);
    }
}
