using AngleSharp.Dom;
using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using PuppeteerSharp.Cdp;
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Devweb.Core
{
    public interface IDemoPupPageRequester : IDisposable
    {
        Task<CrawledPage> MakeRequestAsync(Uri uri);
        Task<CrawledPage> MakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
    }

    public class DemoPupPageRequester : IDemoPupPageRequester
    {
        private readonly CrawlConfiguration _config;
        private readonly IWebContentExtractor _contentExtractor;
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private HttpClientHandler _httpClientHandler;
        private HttpClient _httpClient;
        HttpResponseMessage httpResponseMessage;
        CrawledPage crawledPage;
        BrowserFetcherOptions browserFetcherOptions;
        BrowserFetcher browserFetcher;
        PuppeteerExtra pupExtra;
        ChromiumPuppeteerSharp chromiumPuppeteerSharp;
        IBrowser browserLocal;
        IPage pupPage;
        LaunchOptions launchOptions;
        ConnectOptions pupConnectOptions;
        const string chromeLocalPath = @"c:\browser\1\";
        const string chromiumLocalDirPath = "c:\\browser\\1\\Win64";
        const string downloadPath = @"c:\browser\Chromium\";

        public DemoPupPageRequester(CrawlConfiguration config, IWebContentExtractor contentExtractor, HttpClient httpClient = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _contentExtractor = contentExtractor ?? throw new ArgumentNullException(nameof(contentExtractor));
            if (_config.HttpServicePointConnectionLimit > 0)
                ServicePointManager.DefaultConnectionLimit = _config.HttpServicePointConnectionLimit;
            _httpClient = httpClient;
        }

        public virtual async Task<CrawledPage> MakeRequestAsync(Uri uri)
        {
            return await MakeRequestAsync(uri, (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }


        public virtual async Task<CrawledPage> MakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            // Download Chromium if necessary
            await InstallBrowserLocal();

            if (launchOptions == null)
                BuildLaunchOptions();
            // Launch browser
            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                // Go to www.mail.com
                await page.GoToAsync(uri.ToString());

                // Wait for body to load
                await page.WaitForSelectorAsync("body");

                // Take screenshot
                await page.ScreenshotAsync("mailcom_csharp.png");

                ///////////////////////////////////////
                crawledPage = new CrawledPage(uri);
                crawledPage.RequestStarted = DateTime.Now;
                
                if (_httpClientHandler != null)
                    crawledPage.HttpClientHandler = _httpClientHandler;
                crawledPage.HttpResponseMessage = httpResponseMessage;
                crawledPage.HttpRequestMessage = BuildHttpRequestMessage(uri);
                crawledPage.RequestCompleted = DateTime.Now;



                try
                {
                    //Do anything on page
                }
                catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
                finally
                {
                    try
                    {
                        if (pupPage != null)
                        {
                            var shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                            if (shouldDownloadContentDecision.Allow)
                            {
                                crawledPage.DownloadContentStarted = DateTime.Now;
                                crawledPage.Content = await _contentExtractor.GetContentAsync(httpResponseMessage).ConfigureAwait(false);
                                crawledPage.DownloadContentCompleted = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception e) { }
                }
                return crawledPage;
                ///////////////////////////////////////



                // Close automatically by using statement
            }
            return null;
        }

        public virtual async Task<CrawledPage> TEMPPPPPPPMakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            crawledPage = new CrawledPage(uri);
            crawledPage.RequestStarted = DateTime.Now;
            //if (httpResponseMessage == null)
            //{
            //    httpResponseMessage = new HttpResponseMessage();
            //    if (uri == null)
            //        throw new ArgumentNullException(nameof(uri));
            //    Uri headerUri = new Uri($"{uri.Scheme}://{uri.Authority}");
            //    if (_httpClient == null)
            //    {
            //        _httpClientHandler = BuildHttpClientHandler(uri);
            //        _httpClient = BuildHttpClient(_httpClientHandler);
            //    }
            //    HttpRequestMessage httpRequestMessage;
            //    try
            //    {
            //        using (httpRequestMessage = BuildHttpRequestMessage(headerUri))
            //        {
            //            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, System.Threading.CancellationToken.None).ConfigureAwait(false);
            //        }
            //        var statusCode = Convert.ToInt32(httpResponseMessage.StatusCode);
            //        if (statusCode < 200 || statusCode > 399)
            //            throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
            //    }
            //    catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
            //}
            if (_httpClientHandler != null)
                crawledPage.HttpClientHandler = _httpClientHandler;
            //crawledPage.HttpResponseMessage = httpResponseMessage;
            crawledPage.HttpRequestMessage = BuildHttpRequestMessage(uri);
            crawledPage.RequestCompleted = DateTime.Now;
            if (pupExtra == null) { pupExtra = new PuppeteerExtra(); pupExtra.Use(new StealthPlugin()); }
            await InstallBrowserLocal();
            if (pupConnectOptions == null)
            {
                pupConnectOptions = new ConnectOptions();
                pupConnectOptions.BrowserURL = "http://127.0.0.1:2122";
                pupConnectOptions.DefaultViewport = null;
            }
            try
            {
                if (browserLocal == null)
                    try
                    {
                        browserLocal = await pupExtra.ConnectAsync(pupConnectOptions);
                        browserLocal = await pupExtra.LaunchAsync(launchOptions);
                    }
                    catch (Exception e) { pupExtra = new PuppeteerExtra(); pupExtra.Use(new StealthPlugin()); }
                if (browserLocal == null)
                    browserLocal = await pupExtra.LaunchAsync(launchOptions);
            }
            catch (Exception e) { return null; }

            if (pupPage == null)
                using (pupPage = await browserLocal.NewPageAsync())
                {
                    crawledPage = crawledPage; httpResponseMessage = httpResponseMessage;
                    try
                    {
                        //Do anything on page
                    }
                    catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
                    finally
                    {
                        try
                        {
                            if (pupPage != null)
                            {
                                var shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                                if (shouldDownloadContentDecision.Allow)
                                {
                                    crawledPage.DownloadContentStarted = DateTime.Now;
                                    crawledPage.Content = await _contentExtractor.GetContentAsync(httpResponseMessage).ConfigureAwait(false);
                                    crawledPage.DownloadContentCompleted = DateTime.Now;
                                }
                            }
                        }
                        catch (Exception e) { }
                    }
                    return crawledPage;
                }
            return null;
        }

        protected async Task InstallBrowserLocal()
        {
            chromiumPuppeteerSharp = new ChromiumPuppeteerSharp();
            browserFetcher = await chromiumPuppeteerSharp.GetChromium(downloadPath);
            if (launchOptions == null)
                BuildLaunchOptions();
        }

        private void BuildLaunchOptions()
        {
            if (launchOptions == null)
            {
                launchOptions = new LaunchOptions();
                launchOptions.Browser = SupportedBrowser.Chromium;
                launchOptions.Headless = false;
                launchOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins", "--remote-debugging-port=2122", "--blink-settings=imagesEnabled=false" };
                launchOptions.LogProcess = false;
                launchOptions.DefaultViewport = null;
                launchOptions.ExecutablePath = browserFetcher.GetInstalledBrowsers().First().GetExecutablePath();
                //launchOptions.UserDataDir = 
            }
        }

        protected virtual HttpRequestMessage BuildHttpRequestMessage(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Version = GetEquivalentHttpProtocolVersion();
            return request;
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

        private Version GetEquivalentHttpProtocolVersion()
        {
            if (_config.HttpProtocolVersion == HttpProtocolVersion.Version11)
                return HttpVersion.Version11;
            return HttpVersion.Version20;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _httpClientHandler?.Dispose();
            browserLocal.CloseAsync().Wait();
        }
    }
}