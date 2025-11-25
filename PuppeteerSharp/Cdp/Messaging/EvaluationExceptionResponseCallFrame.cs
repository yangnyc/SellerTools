// <copyright file="EvaluationExceptionResponseCallFrame.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class EvaluationExceptionResponseCallFrame
    {
        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public string Url { get; set; }

        public string FunctionName { get; set; }
    }
}
