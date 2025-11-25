// <copyright file="ClickOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Input
{
    /// <summary>
    /// Options to use when clicking.
    /// </summary>
    public class ClickOptions
    {
        /// <summary>
        /// Gets or sets time to wait between <c>mousedown</c> and <c>mouseup</c> in milliseconds. Defaults to 0.
        /// </summary>
        public int Delay { get; set; } = 0;

        /// <summary>
        /// Gets or sets defaults to 1. See https://developer.mozilla.org/en-US/docs/Web/API/UIEvent/detail.
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// Gets or sets the button to use for the click. Defaults to <see cref="MouseButton.Left"/>.
        /// </summary>
        public MouseButton Button { get; set; } = MouseButton.Left;

        /// <summary>
        /// Gets or sets offset for the clickable point relative to the top-left corner of the border-box.
        /// </summary>
        public Offset? OffSet { get; set; }
    }
}
