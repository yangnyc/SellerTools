// <copyright file="EvaluateHandleResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class EvaluateHandleResponse
    {
        public EvaluateExceptionResponseDetails ExceptionDetails { get; set; }

        public RemoteObject Result { get; set; }
    }
}
