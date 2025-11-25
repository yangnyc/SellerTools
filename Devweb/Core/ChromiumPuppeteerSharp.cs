using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Devweb.Core
{
    public class ChromiumPuppeteerSharp
    {
        BrowserFetcher? browserFetcher;
        InstalledBrowser? installedBrowser;

        public async Task<BrowserFetcher> GetChromium(string downloadPath)
        {
            if (string.IsNullOrEmpty(downloadPath)) return null;
            if (browserFetcher != null) return null;
            browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = downloadPath, Browser = SupportedBrowser.Chromium });

            if (!Directory.Exists(downloadPath) || string.IsNullOrEmpty(browserFetcher.GetInstalledBrowsers().First().GetExecutablePath()))
            {
                if (Directory.Exists(downloadPath))
                {
                    try
                    {
                        //Delete old Directory
                        System.IO.Directory.Delete(downloadPath, true);
                    }
                    catch (Exception ex) { return null; }
                }
                //Create new Directory
                Directory.CreateDirectory(downloadPath);
                await browserFetcher.DownloadAsync();
                //installedBrowser = await browserFetcher.DownloadAsync();
            }
            return browserFetcher;
        }
    }
}
