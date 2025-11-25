// <copyright file="RuntimeCallFunctionOnRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    using System.Collections.Generic;

    internal class RuntimeCallFunctionOnRequest
    {
        public string FunctionDeclaration { get; set; }

        public int? ExecutionContextId { get; set; }

        public object[] Arguments { get; set; }

        public bool ReturnByValue { get; set; }

        public bool AwaitPromise { get; set; }

        public bool UserGesture { get; set; }

        public string ObjectId { get; set; }
    }
}
