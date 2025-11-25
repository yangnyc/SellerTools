// <copyright file="CssQueryHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.QueryHandlers
{
    internal class CssQueryHandler : QueryHandler
    {
        internal CssQueryHandler()
        {
            this.QuerySelector = @"(element, selector) => {
                return element.querySelector(selector);
            }";

            this.QuerySelectorAll = @"(element, selector) => {
                return element.querySelectorAll(selector);
            }";
        }
    }
}
