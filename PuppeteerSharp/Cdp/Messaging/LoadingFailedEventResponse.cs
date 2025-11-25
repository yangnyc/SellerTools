// <copyright file="LoadingFailedEventResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Cdp.Messaging
{
    internal class LoadingFailedEventResponse
    {
        public string RequestId { get; set; }

        public string ErrorText { get; set; }
    }
}
