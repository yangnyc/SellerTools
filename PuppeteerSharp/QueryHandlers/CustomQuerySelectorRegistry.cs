// <copyright file="CustomQuerySelectorRegistry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class CustomQuerySelectorRegistry
    {
        private static readonly string[] CustomQuerySeparators = new[] { "=", "/" };

        private readonly Dictionary<string, QueryHandler> queryHandlers = new();

        private readonly Regex customQueryHandlerNameRegex = new("[a-zA-Z]+$", RegexOptions.Compiled);
        private readonly QueryHandler defaultHandler = new CssQueryHandler();

        internal Dictionary<string, QueryHandler> InternalQueryHandlers => new()
        {
            ["aria"] = new AriaQueryHandler(),
            ["pierce"] = new PierceQueryHandler(),
            ["text"] = new TextQueryHandler(),
            ["xpath"] = new XPathQueryHandler(),
        };

        internal void RegisterCustomQueryHandler(string name, CustomQueryHandler queryHandler)
        {
            if (this.InternalQueryHandlers.ContainsKey(name))
            {
                throw new PuppeteerException($"A query handler named \"{name}\" already exists");
            }

            if (this.queryHandlers.ContainsKey(name))
            {
                throw new PuppeteerException($"A custom query handler named \"{name}\" already exists");
            }

            var isValidName = this.customQueryHandlerNameRegex.IsMatch(name);
            if (!isValidName)
            {
                throw new PuppeteerException($"Custom query handler names may only contain [a-zA-Z]");
            }

            var internalHandler = new QueryHandler
            {
                QuerySelector = queryHandler.QueryOne,
                QuerySelectorAll = queryHandler.QueryAll,
            };

            this.queryHandlers.Add(name, internalHandler);
        }

        internal (string UpdatedSelector, QueryHandler QueryHandler) GetQueryHandlerAndSelector(string selector)
        {
            var handlers = this.InternalQueryHandlers.Concat(this.queryHandlers);

            foreach (var kv in handlers)
            {
                foreach (var separator in CustomQuerySeparators)
                {
                    var prefix = $"{kv.Key}{separator}";

                    if (selector.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        selector = selector.Substring(prefix.Length);
                        return (selector, kv.Value);
                    }
                }
            }

            return (selector, this.defaultHandler);
        }

        internal IEnumerable<string> GetCustomQueryHandlerNames()
            => this.queryHandlers.Keys;

        internal void UnregisterCustomQueryHandler(string name)
            => this.queryHandlers.Remove(name);

        internal void ClearCustomQueryHandlers()
        {
            foreach (var name in this.CustomQueryHandlerNames())
            {
                this.UnregisterCustomQueryHandler(name);
            }
        }

        private IEnumerable<string> CustomQueryHandlerNames() => this.queryHandlers.Keys;
    }
}
