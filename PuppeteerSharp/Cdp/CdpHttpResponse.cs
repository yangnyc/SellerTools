// <copyright file="CdpHttpResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PuppeteerSharp.Cdp.Messaging;

/// <inheritdoc/>
public partial class CdpHttpResponse : Response<CdpHttpRequest>
{
#if NETSTANDARD2_0
    private static readonly Regex _extraInfoLines = new(@"[^ ]* [^ ]* (?<text>.*)", RegexOptions.Multiline);
#endif
    private readonly CDPSession client;
    private readonly bool fromDiskCache;
    private byte[] buffer;

    internal CdpHttpResponse(
        CDPSession client,
        CdpHttpRequest request,
        ResponsePayload responseMessage,
        ResponseReceivedExtraInfoResponse extraInfo)
    {
        this.client = client;
        this.Request = request;
        this.Status = extraInfo != null ? extraInfo.StatusCode : responseMessage.Status;
        this.StatusText = this.ParseStatusTextFromExtraInfo(extraInfo) ?? responseMessage.StatusText;
        this.Url = request.Url;
        this.fromDiskCache = responseMessage.FromDiskCache;
        this.FromServiceWorker = responseMessage.FromServiceWorker;

        this.Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        var headers = extraInfo != null ? extraInfo.Headers : responseMessage.Headers;
        if (headers != null)
        {
            foreach (var keyValue in headers)
            {
                this.Headers[keyValue.Key] = keyValue.Value;
            }
        }

        this.SecurityDetails = responseMessage.SecurityDetails;
        this.RemoteAddress = new RemoteAddress
        {
            IP = responseMessage.RemoteIPAddress,
            Port = responseMessage.RemotePort,
        };
    }

    /// <inheritdoc/>
    public override bool FromCache => this.fromDiskCache || (this.Request?.FromMemoryCache ?? false);

    internal TaskCompletionSource<bool> BodyLoadedTaskWrapper { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Returns a Task which resolves to a buffer with response body.
    /// </summary>
    /// <returns>A Task which resolves to a buffer with response body.</returns>
    public override async ValueTask<byte[]> BufferAsync()
    {
        if (this.buffer == null)
        {
            await this.BodyLoadedTaskWrapper.Task.ConfigureAwait(false);

            try
            {
                var response = await this.client.SendAsync<NetworkGetResponseBodyResponse>("Network.getResponseBody", new NetworkGetResponseBodyRequest
                {
                    RequestId = this.Request.Id,
                }).ConfigureAwait(false);

                this.buffer = response.Base64Encoded
                    ? Convert.FromBase64String(response.Body)
                    : Encoding.UTF8.GetBytes(response.Body);
            }
            catch (Exception ex)
            {
                throw new BufferException("Unable to get response body", ex);
            }
        }

        return this.buffer;
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"[^ ]* [^ ]* (?<text>.*)", RegexOptions.Multiline)]
    private static partial Regex GetExtraInfoLines();
#else
    private static Regex GetExtraInfoLines() => _extraInfoLines;
#endif

    private string ParseStatusTextFromExtraInfo(ResponseReceivedExtraInfoResponse extraInfo)
    {
        if (extraInfo == null || extraInfo.HeadersText == null)
        {
            return null;
        }

        var lines = extraInfo.HeadersText.Split('\r');
        if (lines.Length == 0)
        {
            return null;
        }

        var firstLine = lines[0];

        var match = GetExtraInfoLines().Match(firstLine);
        if (!match.Success)
        {
            return null;
        }

        var statusText = match.Groups["text"].Value;
        return statusText;
    }
}
