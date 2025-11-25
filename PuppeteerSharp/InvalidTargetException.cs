// <copyright file="InvalidTargetException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    [Serializable]
    internal class InvalidTargetException : PuppeteerException
    {
        public InvalidTargetException()
        {
        }

        public InvalidTargetException(string message)
            : base(message)
        {
        }

        public InvalidTargetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
