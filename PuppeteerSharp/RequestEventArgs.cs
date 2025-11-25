// <copyright file="RequestEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Arguments used by <see cref="IPage"/> events.
    /// </summary>
    /// <seealso cref="IPage.Request"/>
    /// <seealso cref="IPage.RequestFailed"/>
    /// <seealso cref="IPage.RequestFinished"/>
    public class RequestEventArgs(IRequest request) : EventArgs
    {
        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>The request.</value>
        public IRequest Request { get; } = request;
    }
}
