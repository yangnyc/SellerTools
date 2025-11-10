using AngleSharp;
using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System;
using System.Net;
using System.Text;

namespace WebApp.Code.Crawler.Demo
{
    public class DemoPupCrawl
    {
        static DemoPageRequester? demoPageRequester;
        PuppeteerExtra? pupExtra;
        BrowserFetcherOptions? browserFetcherOptions;
        IBrowserFetcher? browserFetcher;
        LaunchOptions? pupOptions;
        ConnectOptions? pupConnectOptions;
        IBrowser? pupBrowser;
        IPage? pupPage;
        private readonly CrawlConfiguration _config = new();
        private readonly IWebContentExtractor? _contentExtractor;
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private HttpClientHandler? _httpClientHandler;
        private HttpClient? _httpClient;
        HttpResponseMessage? httpResponseMessage;
        CrawledPage? crawledPage;

        public virtual async Task<bool> CrawlUrl(Uri uri)
        {
            DemoPupPageRequester? demoPupPageRequester = new(GetDefaultCrawlConfiguration(), new WebContentExtractor());
            return IsSuccess(await demoPupPageRequester.MakeRequestAsync(uri, (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false));
        }

        protected virtual HttpClientHandler BuildHttpClientHandler(Uri rootUri)
        {
            if (rootUri == null) throw new ArgumentNullException(nameof(rootUri));
            var httpClientHandler = new HttpClientHandler { MaxAutomaticRedirections = _config.HttpRequestMaxAutoRedirects, UseDefaultCredentials = _config.UseDefaultCredentials };
            if (_config.IsHttpRequestAutomaticDecompressionEnabled) httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (_config.HttpRequestMaxAutoRedirects > 0) httpClientHandler.AllowAutoRedirect = _config.IsHttpRequestAutoRedirectsEnabled;
            if (_config.IsSendingCookiesEnabled) { httpClientHandler.CookieContainer = _cookieContainer; httpClientHandler.UseCookies = true; }
            if (!_config.IsSslCertificateValidationEnabled) { httpClientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; }
            if (_config.IsAlwaysLogin && rootUri != null) { var cache = new CredentialCache(); cache.Add(new Uri($"http://{rootUri.Host}"), "Basic", new NetworkCredential(_config.LoginUser, _config.LoginPassword)); cache.Add(new Uri($"https://{rootUri.Host}"), "Basic", new NetworkCredential(_config.LoginUser, _config.LoginPassword)); httpClientHandler.Credentials = cache; }
            return httpClientHandler;
        }

        protected virtual HttpClient BuildHttpClient(HttpClientHandler clientHandler)
        {
            var httpClient = new HttpClient(clientHandler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgentString);
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            if (_config.HttpRequestTimeoutInSeconds > 0) httpClient.Timeout = TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds);
            if (_config.IsAlwaysLogin) { var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_config.LoginUser + ":" + _config.LoginPassword)); httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials); }
            return httpClient;
        }

        protected virtual HttpRequestMessage BuildHttpRequestMessage(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Version = GetEquivalentHttpProtocolVersion();
            return request;
        }

        private Version GetEquivalentHttpProtocolVersion()
        {
            if (_config.HttpProtocolVersion == HttpProtocolVersion.Version11)
                return HttpVersion.Version11;
            return HttpVersion.Version20;
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

        public async Task<string?> GetUrlHtml(Uri urlToCrawl)
        {
            if (urlToCrawl == null) return null;
            CrawledPage? crawledPage = await GetCrawledPage(urlToCrawl);
            if (crawledPage == null) return null;
            return crawledPage.AngleSharpHtmlDocument.ToHtml();
        }

        public async Task<IHtmlDocument?> GetUrlAngleSharp(Uri urlToCrawl)
        {
            CrawledPage? crawledPage;
            if (urlToCrawl == null) return null;
            crawledPage = await GetCrawledPage(urlToCrawl);
            if (crawledPage == null)
                return null;
            return crawledPage.AngleSharpHtmlDocument;
        }

        private static CrawlConfiguration GetDefaultCrawlConfiguration()
        {
            CrawlConfiguration? defaultCrawlConfiguration = new()
            {
                MaxConcurrentThreads = 1,
                MaxPagesToCrawl = 1,
                MinCrawlDelayPerDomainMilliSeconds = 7000,
                IsSendingCookiesEnabled = true,
                CrawlTimeoutSeconds = 20,
                UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36",
                RobotsDotTextUserAgentString = "false",
                DownloadableContentTypes = "text/html, text/plain, application/json",
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
            return defaultCrawlConfiguration;
        }

        private static async Task<CrawledPage> GetCrawledPage(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            demoPageRequester = new(GetDefaultCrawlConfiguration(), new WebContentExtractor(), null);
            return await demoPageRequester.MakeRequestAsync(uri, (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }
    }
}
