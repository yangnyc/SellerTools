// <copyright file="ScriptInjector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class ScriptInjector
    {
        private static string injectedSource;
        private readonly List<string> amendments = new();
        private bool updated = false;

        public void Append(string statement) => this.Update(() => this.amendments.Add(statement));

        public void Pop(string statement) => this.Update(() => this.amendments.Remove(statement));

        public string Get()
        {
            var amendments = string.Concat(this.amendments.Select(statement => $"({statement})(module.exports.default);"));

            return $@"(() => {{
                const module = {{}};
                {GetInjectedSource()}
                {amendments}
                return module.exports.default;
            }})()";
        }

        public async Task InjectAsync(Func<string, Task> inject, bool force = false)
        {
            if (this.updated || force)
            {
                await inject(this.Get()).ConfigureAwait(false);
            }

            this.updated = false;
        }

        private static string GetInjectedSource()
        {
            if (string.IsNullOrEmpty(injectedSource))
            {
                var assembly = Assembly.GetExecutingAssembly();
                const string resourceName = "PuppeteerSharp.Injected.injected.js";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                var fileContent = reader.ReadToEnd();
                injectedSource = fileContent;
            }

            return injectedSource;
        }

        private void Update(Action callback)
        {
            callback();
            this.updated = true;
        }
    }
}
