// <copyright file="RuntimeGetPropertiesRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class RuntimeGetPropertiesRequest
    {
        public bool OwnProperties { get; set; }

        public string ObjectId { get; set; }
    }
}
