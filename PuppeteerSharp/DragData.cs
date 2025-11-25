// <copyright file="DragData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Data returned by the drag methods.
    /// </summary>
    public partial class DragData
    {
        /// <summary>
        /// Gets or sets drag items.
        /// </summary>
        public DragDataItem[] Items { get; set; }

        /// <summary>
        /// Gets or sets drag operation.
        /// </summary>
        public DragOperation DragOperationsMask { get; set; } = DragOperation.Copy;
    }
}
