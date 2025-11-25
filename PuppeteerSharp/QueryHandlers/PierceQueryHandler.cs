// <copyright file="PierceQueryHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.QueryHandlers
{
    internal class PierceQueryHandler : QueryHandler
    {
        internal PierceQueryHandler()
        {
            this.QuerySelector = @"(element, selector, {pierceQuerySelector}) => {
                return pierceQuerySelector(element, selector);
            }";

            this.QuerySelectorAll = @"(element, selector, {pierceQuerySelectorAll}) => {
                return pierceQuerySelectorAll(element, selector);
            }";
        }
    }
}
