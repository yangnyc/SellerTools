// <copyright file="CdpMouse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Helpers;
using PuppeteerSharp.Helpers.Json;
using PuppeteerSharp.Input;

/// <inheritdoc/>
public class CdpMouse : Mouse
{
    private readonly Keyboard keyboard;
    private readonly MouseState mouseState = new();
    private readonly TaskQueue actionsQueue = new();
    private readonly TaskQueue multipleActionsQueue = new();
    private MouseTransaction.TransactionData inFlightTransaction = null;
    private CDPSession client;

    /// <inheritdoc cref="Mouse"/>
    internal CdpMouse(CDPSession client, Keyboard keyboard)
    {
        this.client = client;
        this.keyboard = keyboard;
    }

    /// <inheritdoc/>
    public override async Task MoveAsync(decimal x, decimal y, MoveOptions options = null)
    {
        options ??= new MoveOptions();

        var position = this.GetState().Position;
        var fromX = position.X;
        var fromY = position.Y;
        var steps = options.Steps;

        for (var i = 1; i <= steps; i++)
        {
            await this.WithTransactionAsync(async (updateState) =>
            {
                updateState(new MouseTransaction.TransactionData
                {
                    Position = new Point
                    {
                        X = fromX + ((x - fromX) * ((decimal)i / steps)),
                        Y = fromY + ((y - fromY) * ((decimal)i / steps)),
                    },
                });

                var state = this.GetState();

                await this.client.SendAsync("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
                {
                    Type = MouseEventType.MouseMoved,
                    Modifiers = this.keyboard.Modifiers,
                    Buttons = (int)state.Buttons,
                    Button = this.GetButtonFromPressedButtons(state.Buttons),
                    X = state.Position.X,
                    Y = state.Position.Y,
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override Task ClickAsync(decimal x, decimal y, ClickOptions options = null)
    {
        options ??= new ClickOptions();

        return this.multipleActionsQueue.Enqueue(async () =>
        {
            if (options.Delay > 0)
            {
                await Task.WhenAll(
                    this.MoveAsync(x, y),
                    this.DownAsync(options)).ConfigureAwait(false);

                await Task.Delay(options.Delay).ConfigureAwait(false);
                await this.UpAsync(options).ConfigureAwait(false);
            }
            else
            {
                await Task.WhenAll(
                this.MoveAsync(x, y),
                this.DownAsync(options),
                this.UpAsync(options)).ConfigureAwait(false);
            }
        });
    }

    /// <inheritdoc/>
    public override Task DownAsync(ClickOptions options = null)
    {
        return this.WithTransactionAsync((updateState) =>
        {
            options ??= new ClickOptions();

            if (this.GetState().Buttons.HasFlag(options.Button))
            {
                throw new PuppeteerException($"{options.Button} is already pressed");
            }

            updateState(new MouseTransaction.TransactionData
            {
                Buttons = this.GetState().Buttons | options.Button,
            });

            var state = this.GetState();
            return this.client.SendAsync("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
            {
                Type = MouseEventType.MousePressed,
                Modifiers = this.keyboard.Modifiers,
                ClickCount = options.Count,
                Buttons = (int)state.Buttons,
                Button = options.Button,
                X = state.Position.X,
                Y = state.Position.Y,
            });
        });
    }

    /// <inheritdoc/>
    public override Task UpAsync(ClickOptions options = null)
    {
        return this.WithTransactionAsync((updateState) =>
        {
            options ??= new ClickOptions();

            if (!this.GetState().Buttons.HasFlag(options.Button))
            {
                throw new PuppeteerException($"{options.Button} is not pressed");
            }

            updateState(new MouseTransaction.TransactionData
            {
                Buttons = this.GetState().Buttons & ~options.Button,
            });

            var state = this.GetState();
            return this.client.SendAsync("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
            {
                Type = MouseEventType.MouseReleased,
                Modifiers = this.keyboard.Modifiers,
                ClickCount = options.Count,
                Buttons = (int)state.Buttons,
                Button = options.Button,
                X = state.Position.X,
                Y = state.Position.Y,
            });
        });
    }

    /// <inheritdoc/>
    public override Task WheelAsync(decimal deltaX, decimal deltaY)
    {
        var state = this.GetState();

        return this.client.SendAsync(
            "Input.dispatchMouseEvent",
            new InputDispatchMouseEventRequest
            {
                Type = MouseEventType.MouseWheel,
                DeltaX = deltaX,
                DeltaY = deltaY,
                X = state.Position.X,
                Y = state.Position.Y,
                Modifiers = this.keyboard.Modifiers,
                PointerType = PointerType.Mouse,
            });
    }

    /// <inheritdoc/>
    public override Task<DragData> DragAsync(decimal startX, decimal startY, decimal endX, decimal endY)
    {
        return this.multipleActionsQueue.Enqueue(async () =>
        {
            var result = new TaskCompletionSource<DragData>();

            void DragIntercepted(object sender, MessageEventArgs e)
            {
                if (e.MessageID == "Input.dragIntercepted")
                {
                    result.TrySetResult(e.MessageData.GetProperty("data").ToObject<DragData>());
                    this.client.MessageReceived -= DragIntercepted;
                }
            }

            this.client.MessageReceived += DragIntercepted;
            await this.MoveAsync(startX, startY).ConfigureAwait(false);
            await this.DownAsync().ConfigureAwait(false);
            await this.MoveAsync(endX, endY).ConfigureAwait(false);

            return await result.Task.ConfigureAwait(false);
        });
    }

    /// <inheritdoc/>
    public override Task DragEnterAsync(decimal x, decimal y, DragData data)
        => this.client.SendAsync(
            "Input.dispatchDragEvent",
            new InputDispatchDragEventRequest
            {
                Type = DragEventType.DragEnter,
                X = x,
                Y = y,
                Modifiers = this.keyboard.Modifiers,
                Data = data,
            });

    /// <inheritdoc/>
    public override Task DragOverAsync(decimal x, decimal y, DragData data)
        => this.client.SendAsync(
            "Input.dispatchDragEvent",
            new InputDispatchDragEventRequest
            {
                Type = DragEventType.DragOver,
                X = x,
                Y = y,
                Modifiers = this.keyboard.Modifiers,
                Data = data,
            });

    /// <inheritdoc/>
    public override Task DropAsync(decimal x, decimal y, DragData data)
        => this.client.SendAsync(
            "Input.dispatchDragEvent",
            new InputDispatchDragEventRequest
            {
                Type = DragEventType.Drop,
                X = x,
                Y = y,
                Modifiers = this.keyboard.Modifiers,
                Data = data,
            });

    /// <inheritdoc/>
    public override async Task DragAndDropAsync(decimal startX, decimal startY, decimal endX, decimal endY, int delay = 0)
    {
        // DragAsync is already using _multipleActionsQueue
        var data = await this.DragAsync(startX, startY, endX, endY).ConfigureAwait(false);
        await this.multipleActionsQueue.Enqueue(async () =>
        {
            await this.DragEnterAsync(endX, endY, data).ConfigureAwait(false);
            await this.DragOverAsync(endX, endY, data).ConfigureAwait(false);

            if (delay > 0)
            {
                await Task.Delay(delay).ConfigureAwait(false);
            }

            await this.DropAsync(endX, endY, data).ConfigureAwait(false);
            await this.UpAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task ResetAsync()
    {
        return this.multipleActionsQueue.Enqueue(() =>
        {
            var actions = new List<Task>();
            var state = this.GetState();

            foreach (var button in new[]
            {
                    MouseButton.Left,
                    MouseButton.Middle,
                    MouseButton.Right,
                    MouseButton.Back,
                    MouseButton.Forward,
            })
            {
                if (state.Buttons.HasFlag(button))
                {
                    actions.Add(this.UpAsync(new()
                    {
                        Button = button,
                    }));
                }
            }

            if (state.Position.X != 0 || state.Position.Y != 0)
            {
                actions.Add(this.MoveAsync(0, 0));
            }

            return Task.WhenAll(actions);
        });
    }

    internal void UpdateClient(CDPSession newSession) => this.client = newSession;

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.actionsQueue.Dispose();
            this.multipleActionsQueue.Dispose();
        }
    }

    private MouseTransaction CreateTransaction()
    {
        this.inFlightTransaction = new MouseTransaction.TransactionData();

        return new MouseTransaction()
        {
            Update = updates =>
            {
                if (updates.Position.HasValue)
                {
                    this.inFlightTransaction.Position = updates.Position.Value;
                }

                if (updates.Buttons.HasValue)
                {
                    this.inFlightTransaction.Buttons = updates.Buttons.Value;
                }
            },
            Commit = () =>
            {
                this.mouseState.Position = this.inFlightTransaction.Position ?? this.mouseState.Position;
                this.mouseState.Buttons = this.inFlightTransaction.Buttons ?? this.mouseState.Buttons;
                this.inFlightTransaction = null;
            },
            Rollback = () => this.inFlightTransaction = null,
        };
    }

    private Task WithTransactionAsync(Func<Action<MouseTransaction.TransactionData>, Task> action)
    {
        return this.actionsQueue.Enqueue(async () =>
        {
            var transaction = this.CreateTransaction();
            try
            {
                await action(transaction.Update).ConfigureAwait(false);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new PuppeteerException("Failed to perform mouse action", ex);
            }
        });
    }

    private MouseButton GetButtonFromPressedButtons(MouseButton buttons)
    {
        if (buttons.HasFlag(MouseButton.Left))
        {
            return MouseButton.Left;
        }

        if (buttons.HasFlag(MouseButton.Right))
        {
            return MouseButton.Right;
        }

        if (buttons.HasFlag(MouseButton.Middle))
        {
            return MouseButton.Middle;
        }

        if (buttons.HasFlag(MouseButton.Back))
        {
            return MouseButton.Back;
        }

        if (buttons.HasFlag(MouseButton.Forward))
        {
            return MouseButton.Forward;
        }

        return MouseButton.None;
    }

    private MouseState GetState()
    {
        var state = new MouseTransaction.TransactionData()
        {
            Position = this.mouseState.Position,
            Buttons = this.mouseState.Buttons,
        };

        if (this.inFlightTransaction != null)
        {
            if (this.inFlightTransaction.Position.HasValue)
            {
                state.Position = this.inFlightTransaction.Position.Value;
            }

            if (this.inFlightTransaction.Buttons.HasValue)
            {
                state.Buttons = this.inFlightTransaction.Buttons.Value;
            }
        }

        return new MouseState
        {
            Position = state.Position.Value,
            Buttons = state.Buttons.Value,
        };
    }
}
