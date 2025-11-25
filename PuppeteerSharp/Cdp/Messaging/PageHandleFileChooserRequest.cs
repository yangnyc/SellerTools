// <copyright file="PageHandleFileChooserRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class PageHandleFileChooserRequest
    {
        public FileChooserAction Action { get; set; }

        public IEnumerable<string> Files { get; set; }
    }
}
