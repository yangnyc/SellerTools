// <copyright file="EmulateIdleOverrides.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    /// Options for <see cref="IPage.EmulateIdleStateAsync(EmulateIdleOverrides)"/>.
    /// </summary>
    public class EmulateIdleOverrides
    {
        /// <summary>
        /// Gets or sets a value indicating whether whether the user is active or not.
        /// </summary>
        public bool IsUserActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the screen is unlocked or not.
        /// </summary>
        public bool IsScreenUnlocked { get; set; }
    }
}
