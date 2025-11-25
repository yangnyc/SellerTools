// <copyright file="RuntimeGetPropertiesResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class RuntimeGetPropertiesResponse
    {
        public IEnumerable<RuntimeGetPropertiesResponseItem> Result { get; set; }

        public class RuntimeGetPropertiesResponseItem
        {
            public object Enumerable { get; set; }

            public string Name { get; set; }

            public RemoteObject Value { get; set; }
        }
    }
}
