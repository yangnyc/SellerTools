// <copyright file="TargetInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// Target info.
    /// </summary>
    public class TargetInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public TargetType Type { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the target identifier.
        /// </summary>
        /// <value>The target identifier.</value>
        public string TargetId { get; set; }

        /// <summary>
        /// Gets or sets the target browser contextId.
        /// </summary>
        [JsonConverter(typeof(AnyTypeToStringConverter))]
        public string BrowserContextId { get; set; }

        /// <summary>
        /// Gets or sets get the target that opened this target.
        /// </summary>
        public string OpenerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets whether the target is attached.
        /// </summary>
        public bool Attached { get; set; }

        /// <summary>
        /// Gets or sets provides additional details for specific target types. For example, for
        /// the type of "page", this may be set to "portal" or "prerender".
        /// </summary>
        public string Subtype { get; set; }
    }
}
