// <copyright file="TargetChangedArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    /// <summary>
    ///  Event arguments used by target related events.
    /// </summary>
    /// <seealso cref="IBrowser.TargetChanged"/>
    /// <seealso cref="IBrowser.TargetCreated"/>
    /// <seealso cref="IBrowser.TargetDestroyed"/>
    public class TargetChangedArgs
    {
        private TargetInfo targetInfo;

        /// <summary>
        /// Gets the target info.
        /// </summary>
        /// <value>The target info.</value>
        public TargetInfo TargetInfo
        {
            get => this.targetInfo ?? this.Target.TargetInfo;
            internal set => this.targetInfo = value;
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        public Target Target { get; internal set; }
    }
}
