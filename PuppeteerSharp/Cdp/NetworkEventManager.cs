// <copyright file="NetworkEventManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using PuppeteerSharp.Cdp.Messaging;

    internal class NetworkEventManager
    {
        private readonly ConcurrentDictionary<string, RequestWillBeSentResponse> requestWillBeSentMap = new();
        private readonly ConcurrentDictionary<string, FetchRequestPausedResponse> requestPausedMap = new();
        private readonly ConcurrentDictionary<string, CdpHttpRequest> httpRequestsMap = new();
        private readonly ConcurrentDictionary<string, QueuedEventGroup> queuedEventGroupMap = new();
        private readonly ConcurrentDictionary<string, List<RedirectInfo>> queuedRedirectInfoMap = new();
        private readonly ConcurrentDictionary<string, List<ResponseReceivedExtraInfoResponse>> responseReceivedExtraInfoMap = new();

        public int NumRequestsInProgress
            => this.httpRequestsMap.Values.Count(r => r.Response == null);

        internal void Forget(string requestId)
        {
            this.requestWillBeSentMap.TryRemove(requestId, out _);
            this.requestPausedMap.TryRemove(requestId, out _);
            this.queuedEventGroupMap.TryRemove(requestId, out _);
            this.queuedRedirectInfoMap.TryRemove(requestId, out _);
            this.responseReceivedExtraInfoMap.TryRemove(requestId, out _);
            this.httpRequestsMap.TryRemove(requestId, out _);
        }

        internal List<ResponseReceivedExtraInfoResponse> ResponseExtraInfo(string networkRequestId)
            => this.responseReceivedExtraInfoMap.GetOrAdd(networkRequestId, static _ => new());

        internal void QueueRedirectInfo(string fetchRequestId, RedirectInfo redirectInfo)
            => this.QueuedRedirectInfo(fetchRequestId).Add(redirectInfo);

        internal RedirectInfo TakeQueuedRedirectInfo(string fetchRequestId)
        {
            var list = this.QueuedRedirectInfo(fetchRequestId);
            var result = list.FirstOrDefault();

            if (result != null)
            {
                list.Remove(result);
            }

            return result;
        }

        internal ResponseReceivedExtraInfoResponse ShiftResponseExtraInfo(string networkRequestId)
        {
            var list = this.responseReceivedExtraInfoMap.GetOrAdd(networkRequestId, static _ => new());
            var result = list.FirstOrDefault();

            if (result != null)
            {
                list.Remove(result);
            }

            return result;
        }

        internal void StoreRequestWillBeSent(string networkRequestId, RequestWillBeSentResponse e)
            => this.requestWillBeSentMap.AddOrUpdate(networkRequestId, e, (_, _) => e);

        internal RequestWillBeSentResponse GetRequestWillBeSent(string networkRequestId)
        {
            this.requestWillBeSentMap.TryGetValue(networkRequestId, out var result);
            return result;
        }

        internal void ForgetRequestWillBeSent(string networkRequestId)
            => this.requestWillBeSentMap.TryRemove(networkRequestId, out _);

        internal FetchRequestPausedResponse GetRequestPaused(string networkRequestId)
        {
            this.requestPausedMap.TryGetValue(networkRequestId, out var result);
            return result;
        }

        internal void ForgetRequestPaused(string networkRequestId)
            => this.requestPausedMap.TryRemove(networkRequestId, out _);

        internal void StoreRequestPaused(string networkRequestId, FetchRequestPausedResponse e)
            => this.requestPausedMap.AddOrUpdate(networkRequestId, e, (_, _) => e);

        internal CdpHttpRequest GetRequest(string networkRequestId)
        {
            this.httpRequestsMap.TryGetValue(networkRequestId, out var result);
            return result;
        }

        internal void StoreRequest(string networkRequestId, CdpHttpRequest request)
            => this.httpRequestsMap.AddOrUpdate(networkRequestId, request, (_, _) => request);

        internal void ForgetRequest(string requestId)
            => this.requestWillBeSentMap.TryRemove(requestId, out _);

        internal void QueuedEventGroup(string networkRequestId, QueuedEventGroup group)
            => this.queuedEventGroupMap.AddOrUpdate(networkRequestId, group, (_, _) => group);

        internal QueuedEventGroup GetQueuedEventGroup(string networkRequestId)
        {
            this.queuedEventGroupMap.TryGetValue(networkRequestId, out var result);
            return result;
        }

        // Puppeteer doesn't have this. but it seems that .NET needs this to avoid race conditions
        internal void ForgetQueuedEventGroup(string networkRequestId)
            => this.queuedEventGroupMap.TryRemove(networkRequestId, out _);

        private List<RedirectInfo> QueuedRedirectInfo(string fetchRequestId)
            => this.queuedRedirectInfoMap.GetOrAdd(fetchRequestId, static _ => new());
    }
}
