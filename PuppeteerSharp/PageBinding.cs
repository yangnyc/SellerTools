// <copyright file="PageBinding.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    internal class PageBinding
    {
        public string Name { get; set; }

        public Delegate Function { get; set; }
    }
}
