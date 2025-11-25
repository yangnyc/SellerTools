// <copyright file="IFrameProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp;

using System.Threading.Tasks;

/// <summary>
/// Provides a way to get a frame.
/// </summary>
internal interface IFrameProvider
{
    /// <summary>
    /// Gets a frame by its ID.
    /// </summary>
    /// <param name="frameId">The frame ID.</param>
    /// <returns>A <see cref="Task"/> that completes when the frame is retrieved.</returns>
    Task<CdpFrame> GetFrameAsync(string frameId);
}
