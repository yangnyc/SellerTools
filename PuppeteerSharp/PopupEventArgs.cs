// <copyright file="PopupEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Popup event arguments. <see cref="IPage.Popup"/>.
    /// </summary>
    public class PopupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the popup page.
        /// </summary>
        public IPage PopupPage { get; internal set; }
    }
}
