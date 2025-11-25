// <copyright file="DragDataItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Item returned by drag methods.
    /// </summary>
    public class DragDataItem
    {
        /// <summary>
        /// Gets or sets mime type of the dragged data.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets depending of the value of `mimeType`, it contains the dragged link, text, HTML markup or any other data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets title associated with a link. Only valid when `mimeType` == "text/uri-list".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets stores the base URL for the contained markup. Only valid when `mimeType` == "text/html".
        /// </summary>
        public string BaseURL { get; set; }
    }
}
