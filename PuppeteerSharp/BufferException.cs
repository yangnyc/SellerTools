// <copyright file="BufferException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    [Serializable]
    internal class BufferException : PuppeteerException
    {
        public BufferException()
        {
        }

        public BufferException(string message)
            : base(message)
        {
        }

        public BufferException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
