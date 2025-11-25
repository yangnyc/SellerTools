// <copyright file="Cache.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.BrowserData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using PuppeteerSharp.Helpers;

    internal class Cache
    {
        private readonly string rootDir;

        public Cache() => this.rootDir = BrowserFetcher.GetBrowsersLocation();

        public Cache(string rootDir) => this.rootDir = rootDir;

        public string GetBrowserRoot(SupportedBrowser browser) => Path.Combine(this.rootDir, browser.ToString());

        public string GetInstallationDir(SupportedBrowser browser, Platform platform, string buildId)
            => Path.Combine(this.GetBrowserRoot(browser), $"{platform}-{buildId}");

        public IEnumerable<InstalledBrowser> GetInstalledBrowsers()
        {
            var rootInfo = new DirectoryInfo(this.rootDir);

            if (!rootInfo.Exists)
            {
                return Array.Empty<InstalledBrowser>();
            }

            var browserNames = EnumHelper.GetNames<SupportedBrowser>().Select(browser => browser.ToUpperInvariant());
            var browsers = rootInfo.GetDirectories().Where(browser => browserNames.Contains(browser.Name.ToUpperInvariant()));

            return browsers.SelectMany(browser =>
            {
                var browserEnum = EnumHelper.Parse<SupportedBrowser>(browser.Name, ignoreCase: true);
                var dirInfo = new DirectoryInfo(this.GetBrowserRoot(browserEnum));
                var dirs = dirInfo.GetDirectories();

                return dirs.Select(dir =>
                {
                    var result = this.ParseFolderPath(dir);

                    if (result == null)
                    {
                        return null;
                    }

                    var platformEnum = EnumHelper.Parse<Platform>(result.Value.Platform, ignoreCase: true);
                    return new InstalledBrowser(this, browserEnum, result.Value.BuildId, platformEnum);
                })
                .Where(item => item != null);
            });
        }

        public void Uninstall(SupportedBrowser browser, Platform platform, string buildId)
        {
            var dir = new DirectoryInfo(this.GetInstallationDir(browser, platform, buildId));
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }

        public void Clear() => new DirectoryInfo(this.rootDir).Delete(true);

        private (string Platform, string BuildId)? ParseFolderPath(DirectoryInfo directory)
        {
            var name = directory.Name;
            var splits = name.Split('-');

            if (splits.Length != 2)
            {
                return null;
            }

            return (splits[0], splits[1]);
        }
    }
}
