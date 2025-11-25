// <copyright file="DialogEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// <see cref="IPage.Dialog"/> arguments.
    /// </summary>
    public class DialogEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogEventArgs"/> class.
        /// </summary>
        /// <param name="dialog">Dialog.</param>
        public DialogEventArgs(Dialog dialog) => this.Dialog = dialog;

        /// <summary>
        /// Gets dialog data.
        /// </summary>
        public Dialog Dialog { get; }
    }
}
