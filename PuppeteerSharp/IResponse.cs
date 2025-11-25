// <copyright file="IResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="IResponse"/> class represents responses which are received by page.
    /// </summary>
    /// <seealso cref="IPage.GoForwardAsync(NavigationOptions)"/>
    /// <seealso cref="IPage.ReloadAsync(int?, WaitUntilNavigation[])"/>
    /// <seealso cref="IPage.Response"/>
    /// <seealso cref="IPage.WaitForResponseAsync(System.Func{PuppeteerSharp.IResponse,bool}, WaitForOptions)"/>
    public interface IResponse
    {
        /// <summary>
        /// Gets contains the URL of the response.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets an object with HTTP headers associated with the response. All header names are lower-case.
        /// </summary>
        /// <value>The headers.</value>
        Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets contains the status code of the response.
        /// </summary>
        /// <value>The status.</value>
        HttpStatusCode Status { get; }

        /// <summary>
        /// Gets a value indicating whether contains a boolean stating whether the response was successful (status in the range 200-299) or not.
        /// </summary>
        /// <value><c>true</c> if ok; otherwise, <c>false</c>.</value>
        bool Ok { get; }

        /// <summary>
        /// Gets a matching <see cref="Request"/> object.
        /// </summary>
        /// <value>The request.</value>
        IRequest Request { get; }

        /// <summary>
        /// Gets a value indicating whether true if the response was served from either the browser's disk cache or memory cache.
        /// </summary>
        bool FromCache { get; }

        /// <summary>
        /// Gets the security details.
        /// </summary>
        /// <value>The security details.</value>
        SecurityDetails SecurityDetails { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IResponse"/> was served by a service worker.
        /// </summary>
        /// <value><c>true</c> if the <see cref="IResponse"/> was served by a service worker; otherwise, <c>false</c>.</value>
        bool FromServiceWorker { get; }

        /// <summary>
        /// Gets contains the status text of the response (e.g. usually an "OK" for a success).
        /// </summary>
        /// <value>The status text.</value>
        string StatusText { get; }

        /// <summary>
        /// Gets remove server address.
        /// </summary>
        RemoteAddress RemoteAddress { get; }

        /// <summary>
        /// Gets a <see cref="Frame"/> that initiated this request. Or null if navigating to error pages.
        /// </summary>
        IFrame Frame { get; }

        /// <summary>
        /// Returns a Task which resolves to a buffer with response body.
        /// </summary>
        /// <returns>A Task which resolves to a buffer with response body.</returns>
        ValueTask<byte[]> BufferAsync();

        /// <summary>
        /// Returns a Task which resolves to a text representation of response body.
        /// </summary>
        /// <returns>A Task which resolves to a text representation of response body.</returns>
        Task<string> TextAsync();

        /// <summary>
        /// Returns a Task which resolves to a <see cref="JsonDocument"/> representation of response body.
        /// </summary>
        /// <param name="options">Parsing options.</param>
        /// <seealso cref="JsonAsync{T}"/>
        /// <returns>A Task which resolves to a <see cref="JsonDocument"/> representation of response body.</returns>
        Task<JsonDocument> JsonAsync(JsonDocumentOptions options = default);

        /// <summary>
        /// Returns a Task which resolves to a <typeparamref name="T"/> representation of response body.
        /// </summary>
        /// <param name="options">Parsing options.</param>
        /// <typeparam name="T">The type of the response.</typeparam>
        /// <seealso cref="JsonAsync"/>
        /// <returns>A Task which resolves to a <typeparamref name="T"/> representation of response body.</returns>
        Task<T> JsonAsync<T>(JsonSerializerOptions options = default);
    }
}
