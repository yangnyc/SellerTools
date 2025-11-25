// <copyright file="TargetCrashedException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    [Serializable]
    internal class TargetCrashedException : PuppeteerException
    {
        public TargetCrashedException()
        {
        }

        public TargetCrashedException(string message)
            : base(message)
        {
        }

        public TargetCrashedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
