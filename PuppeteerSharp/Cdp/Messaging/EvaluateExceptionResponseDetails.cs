// <copyright file="EvaluateExceptionResponseDetails.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class EvaluateExceptionResponseDetails
    {
        public EvaluateExceptionResponseInfo Exception { get; set; }

        public string Text { get; set; }

        public EvaluateExceptionResponseStackTrace StackTrace { get; set; }
    }
}
