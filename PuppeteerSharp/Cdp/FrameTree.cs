// <copyright file="FrameTree.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;

    internal class FrameTree
    {
        private readonly AsyncDictionaryHelper<string, CdpFrame> frames = new("Frame {0} not found");
        private readonly ConcurrentDictionary<string, string> parentIds = new();
        private readonly ConcurrentDictionary<string, List<string>> childIds = new();
        private readonly ConcurrentDictionary<string, List<TaskCompletionSource<CdpFrame>>> waitRequests = new();

        public CdpFrame MainFrame { get; set; }

        public CdpFrame[] Frames => this.frames.Values.ToArray();

        internal Task<CdpFrame> GetFrameAsync(string frameId) => this.frames.GetItemAsync(frameId);

        internal Task<CdpFrame> TryGetFrameAsync(string frameId) => this.frames.TryGetItemAsync(frameId);

        internal CdpFrame GetById(string id)
        {
            this.frames.TryGetValue(id, out var result);
            return result;
        }

        internal Task<CdpFrame> WaitForFrameAsync(string frameId)
        {
            var frame = this.GetById(frameId);
            if (frame != null)
            {
                return Task.FromResult(frame);
            }

            var deferred = new TaskCompletionSource<CdpFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbacks = this.waitRequests.GetOrAdd(frameId, static _ => new());
            callbacks.Add(deferred);

            return deferred.Task;
        }

        internal void AddFrame(CdpFrame frame)
        {
            this.frames.AddItem(frame.Id, frame);
            if (frame.ParentId != null)
            {
                this.parentIds.TryAdd(frame.Id, frame.ParentId);

                var childIds = this.childIds.GetOrAdd(frame.ParentId, static _ => new());
                childIds.Add(frame.Id);
            }
            else
            {
                this.MainFrame = frame;
            }

            this.waitRequests.TryGetValue(frame.Id, out var requests);

            if (requests != null)
            {
                foreach (var request in requests)
                {
                    request.TrySetResult(frame);
                }
            }
        }

        internal void RemoveFrame(Frame frame)
        {
            this.frames.TryRemove(frame.Id, out _);
            this.parentIds.TryRemove(frame.Id, out var _);

            if (frame.ParentId != null)
            {
                this.childIds.TryGetValue(frame.ParentId, out var childs);
                childs?.Remove(frame.Id);
            }
            else
            {
                this.MainFrame = null;
            }
        }

        internal CdpFrame[] GetChildFrames(string frameId)
        {
            this.childIds.TryGetValue(frameId, out var childIds);
            if (childIds == null)
            {
                return Array.Empty<CdpFrame>();
            }

            return childIds
                .Select(id => this.GetById(id))
                .Where(frame => frame != null)
                .ToArray();
        }

        internal Frame GetParentFrame(string frameId)
        {
            this.parentIds.TryGetValue(frameId, out var parentId);
            return !string.IsNullOrEmpty(parentId) ? this.GetById(parentId) : null;
        }
    }
}
