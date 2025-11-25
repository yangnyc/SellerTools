// <copyright file="FrameNavigatedEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using PuppeteerSharp.Cdp.Messaging;

/// <summary>
/// <see cref="IPage.FrameNavigated"/> arguments.
/// </summary>
public record FrameNavigatedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameNavigatedEventArgs"/> class.
    /// </summary>
    /// <param name="frame">Frame.</param>
    /// <param name="type">Navigation type.</param>
    internal FrameNavigatedEventArgs(IFrame frame, NavigationType type)
    {
        this.Frame = frame;
        this.Type = type;
    }

    /// <summary>
    /// Gets or sets the frame.
    /// </summary>
    /// <value>The frame.</value>
    public IFrame Frame { get; set; }

    /// <summary>
    /// Gets navigation type.
    /// </summary>
    public NavigationType Type { get; }
}
