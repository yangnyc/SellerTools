using AngleSharp;
using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System.Net;

namespace WebApp.Code.Crawler.Demo
{
    public class DemoPupCrawl
    {
        static DemoPupPageRequester? demoPupPageRequester;
        PuppeteerExtra? puppeteerExtra;
        BrowserFetcherOptions? browserFetcherOptions;
        IBrowserFetcher? browserFetcher;
        LaunchOptions? puppeteerOptions;
        IBrowser? puppeteerBrowser;
        const string chromeLocalPath1 = @"c:\browser\1\";
        const string chromiumLocalDirPath1 = "c:\\browser\\1\\Win64-884014";


        public async Task<bool> GoTo(Uri _uri)
        {
            CrawledPage crawledPage;

            crawledPage = await GetCrawledPage(_uri.AbsoluteUri);
            if (!IsSuccess(crawledPage)) return false;
            return true;
        }

        private bool IsSuccess(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            return true;
        }

        public async Task<string?> GetUrlHtml(string urlToCrawl)
        {
            if (urlToCrawl == null)
                return null;
            CrawledPage? crawledPage = await GetCrawledPage(urlToCrawl);
            if (crawledPage == null)
                return null;
            return crawledPage.AngleSharpHtmlDocument.ToHtml();
        }

        public async Task<IHtmlDocument?> GetUrlAngleSharp(string urlToCrawl)
        {
            CrawledPage? crawledPage;

            if (urlToCrawl == null)
                return null;
            crawledPage = await GetCrawledPage(urlToCrawl);
            if (crawledPage == null)
                return null;
            return crawledPage.AngleSharpHtmlDocument;
        }

        private static async Task<CrawledPage> GetCrawledPage(string _uri)
        {
            CrawlConfiguration? crawlConfiguration = new()
            {
                MaxConcurrentThreads = 1,
                MaxPagesToCrawl = 1,
                MinCrawlDelayPerDomainMilliSeconds = 7000,
                IsSendingCookiesEnabled = true,
                CrawlTimeoutSeconds = 20,
                UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36",
                RobotsDotTextUserAgentString = "false",
                DownloadableContentTypes = "text/html, text/plain, application/json",
                //ConfigurationExtensions = new System.Collections.Generic.Dictionary<string, string>(),
                //MaxRobotsDotTextCrawlDelayInSeconds = 5,
                HttpRequestMaxAutoRedirects = 1,
                IsHttpRequestAutoRedirectsEnabled = true,
                MaxCrawlDepth = 0,
                HttpServicePointConnectionLimit = 200,
                HttpRequestTimeoutInSeconds = 20,
                IsSslCertificateValidationEnabled = true,
                IsExternalPageCrawlingEnabled = false,
                IsExternalPageLinksCrawlingEnabled = false,
                IsForcedLinkParsingEnabled = false,
                IsIgnoreRobotsDotTextIfRootDisallowedEnabled = false,
                IsRespectHttpXRobotsTagHeaderNoFollowEnabled = true,
                IsRespectRobotsDotTextEnabled = true,
                IsRespectUrlNamedAnchorOrHashbangEnabled = true,
                IsUriRecrawlingEnabled = false,
                IsHttpRequestAutomaticDecompressionEnabled = true,
                IsRespectMetaRobotsNoFollowEnabled = true,
                IsRespectAnchorRelNoFollowEnabled = true,
            };

            if (demoPupPageRequester == null)
                demoPupPageRequester = new(crawlConfiguration, new WebContentExtractor(), null);
            return await demoPupPageRequester.MakeRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }
    }
}
