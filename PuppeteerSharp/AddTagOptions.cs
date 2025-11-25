// <copyright file="AddTagOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Options used by <see cref="IPage.AddScriptTagAsync(AddTagOptions)"/> &amp; <see cref="IPage.AddStyleTagAsync(AddTagOptions)"/>.
    /// </summary>
    public class AddTagOptions
    {
        /// <summary>
        /// Gets or sets url of a script to be added.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets path to the JavaScript file to be injected into frame. If its a relative path, then it is resolved relative to <see cref="System.IO.Directory.GetCurrentDirectory"/>.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets javaScript to be injected into the frame.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets script type. Use <c>module</c> in order to load a Javascript ES2015 module.
        /// </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script"/>
        public string Type { get; set; } = "text/javascript";

        /// <summary>
        /// Gets or sets id of the script.
        /// </summary>
        public object Id { get; set; }
    }
}
