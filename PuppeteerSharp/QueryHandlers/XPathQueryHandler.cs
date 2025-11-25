// <copyright file="XPathQueryHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.QueryHandlers
{
    internal class XPathQueryHandler : QueryHandler
    {
        internal XPathQueryHandler()
        {
            this.QuerySelectorAll = @"(element, selector, {xpathQuerySelectorAll}) => {
                return xpathQuerySelectorAll(element, selector);
            }";
        }
    }
}
