// <copyright file="TextQueryHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.QueryHandlers
{
    internal class TextQueryHandler : QueryHandler
    {
        internal TextQueryHandler()
        {
            this.QuerySelectorAll = @"(element, selector, {textQuerySelectorAll}) => {
                return textQuerySelectorAll(element, selector);
            }";
        }
    }
}
