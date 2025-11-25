// <copyright file="ResponseCreatedEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// <see cref="IPage.Response"/> arguments.
    /// </summary>
    public class ResponseCreatedEventArgs(IResponse response) : EventArgs
    {
        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>The response.</value>
        public IResponse Response { get; } = response;
    }
}
