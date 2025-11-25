// <copyright file="CdpElementHandle.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.QueryHandlers;

/// <inheritdoc />
public class CdpElementHandle : ElementHandle
{
    private readonly CdpFrame cdpFrame;

    internal CdpElementHandle(
        IsolatedWorld world,
        RemoteObject remoteObject)
    {
        this.Handle = new CdpJSHandle(world, remoteObject);
        this.Logger = this.Realm.Environment.Client.Connection.LoggerFactory.CreateLogger(this.GetType());
        this.cdpFrame = this.Realm.Frame as CdpFrame;
    }

    /// <inheritdoc />
    public override RemoteObject RemoteObject => this.Handle.RemoteObject;

    internal override IsolatedWorld Realm => this.Handle.Realm;

    /// <summary>
    /// Gets logger.
    /// </summary>
    internal ILogger Logger { get; }

    internal override CustomQuerySelectorRegistry CustomQuerySelectorRegistry =>
        this.Client.Connection.CustomQuerySelectorRegistry;

    /// <inheritdoc/>
    protected override Page Page => this.cdpFrame.FrameManager.Page;

    private CDPSession Client => this.Handle.Realm.Environment.Client;

    private FrameManager FrameManager => this.cdpFrame.FrameManager;

    /// <inheritdoc/>
    public override async Task<IFrame> ContentFrameAsync()
    {
        var nodeInfo = await this.Client
            .SendAsync<DomDescribeNodeResponse>("DOM.describeNode", new DomDescribeNodeRequest { ObjectId = this.Id, })
            .ConfigureAwait(false);

        return string.IsNullOrEmpty(nodeInfo.Node.FrameId)
            ? null
            : await this.FrameManager.FrameTree.GetFrameAsync(nodeInfo.Node.FrameId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task ScrollIntoViewAsync()
        => this.BindIsolatedHandleAsync<IElementHandle, CdpElementHandle>(async handle =>
        {
            await handle.AssertConnectedElementAsync().ConfigureAwait(false);
            try
            {
                await handle.Client
                    .SendAsync(
                        "DOM.scrollIntoViewIfNeeded",
                        new DomScrollIntoViewIfNeededRequest { ObjectId = this.Id, })
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "DOM.scrollIntoViewIfNeeded is not supported");
                await base.ScrollIntoViewAsync().ConfigureAwait(false);
            }

            return handle;
        });

    /// <inheritdoc />
    public override Task UploadFileAsync(bool resolveFilePaths, params string[] filePaths)
            => this.BindIsolatedHandleAsync<CdpElementHandle, CdpElementHandle>(async handle =>
            {
                var isMultiple = await this.EvaluateFunctionAsync<bool>("element => element.multiple").ConfigureAwait(false);

                if (!isMultiple && filePaths.Length > 1)
                {
                    throw new PuppeteerException("Multiple file uploads only work with <input type=file multiple>");
                }

                // The zero-length array is a special case, it seems that
                // DOM.setFileInputFiles does not actually update the files in that case, so
                // the solution is to eval the element value to a new FileList directly.
                if (filePaths.Length == 0)
                {
                    await handle.EvaluateFunctionAsync(@"(element) => {
                        element.files = new DataTransfer().files;

                        // Dispatch events for this case because it should behave akin to a user action.
                        element.dispatchEvent(
                            new Event('input', {bubbles: true, composed: true})
                        );
                        element.dispatchEvent(new Event('change', { bubbles: true }));
                    }").ConfigureAwait(false);

                    return handle;
                }

                var node = await handle.Client
                    .SendAsync<DomDescribeNodeResponse>(
                        "DOM.describeNode",
                        new DomDescribeNodeRequest { ObjectId = this.Id, }).ConfigureAwait(false);
                var backendNodeId = node.Node.BackendNodeId;

                var files = resolveFilePaths ? filePaths.Select(Path.GetFullPath).ToArray() : filePaths;
                this.CheckForFileAccess(files);
                await handle.Client.SendAsync(
                    "DOM.setFileInputFiles",
                    new DomSetFileInputFilesRequest
                    {
                        ObjectId = this.Id,
                        Files = files,
                        BackendNodeId = backendNodeId,
                    })
                    .ConfigureAwait(false);

                return handle;
            });

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (this.Disposed)
        {
            return;
        }

        this.Disposed = true;

        await this.Handle.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override string ToString() => this.Handle.ToString();

    private void CheckForFileAccess(string[] files)
    {
        foreach (var file in files)
        {
            try
            {
                File.Open(file, FileMode.Open).Dispose();
            }
            catch (Exception ex)
            {
                throw new PuppeteerException($"{files} does not exist or is not readable", ex);
            }
        }
    }
}
