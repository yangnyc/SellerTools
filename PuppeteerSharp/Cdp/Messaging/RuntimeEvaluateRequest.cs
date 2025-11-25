// <copyright file="RuntimeEvaluateRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class RuntimeEvaluateRequest
    {
        public string Expression { get; set; }

        public bool AwaitPromise { get; set; }

        public bool ReturnByValue { get; set; }
    }
}
