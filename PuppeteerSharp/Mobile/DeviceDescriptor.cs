// <copyright file="DeviceDescriptor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Mobile
{
    /// <summary>
    /// Device descriptor.
    /// </summary>
    public class DeviceDescriptor
    {
        /// <summary>
        /// Gets device name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets user Agent.
        /// </summary>
        /// <value>The user agent.</value>
        public string UserAgent { get; internal set; }

        /// <summary>
        /// Gets viewPort.
        /// </summary>
        /// <value>The view port.</value>
        public ViewPortOptions ViewPort { get; internal set; }
    }
}
