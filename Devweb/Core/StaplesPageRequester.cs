using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Devweb.Core
{
    public interface IStaplesPageRequester : IDisposable
    {
        /// <summary>
        /// Make an http web request to the url and download its content
        /// </summary>
        Task<CrawledPage> MakeRequestAsync(Uri uri);

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// </summary>
        Task<CrawledPage> MakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
    }

    public class StaplesPageRequester : IStaplesPageRequester
    {
        private readonly CrawlConfiguration _config;
        private readonly IWebContentExtractor _contentExtractor;
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private HttpClientHandler _httpClientHandler;
        private HttpClient _httpClient;
        HttpResponseMessage httpResponseMessage;
        PuppeteerExtra puppeteerExtra;
        int countNum;
        BrowserFetcherOptions browserFetcherOptions1;
        BrowserFetcher browserFetcher1;
        BrowserFetcherOptions browserFetcherOptions2;
        BrowserFetcher browserFetcher2;
        BrowserFetcherOptions browserFetcherOptions;
        BrowserFetcher browserFetcher;
        IResponse puppeteerResponse;
        IPage puppeteerPage;
        IPage puppeteerPage2;
        IPage puppeteerPage3;
        IPage puppeteerPage4;
        IPage puppeteerPage5;
        IPage puppeteerPage6;
        LaunchOptions puppeteerOptions;
        IBrowser puppeteerBrowser;
        const string chromeLocalPath1 = @"c:\browser\1\";
        const string chromeLocalPath2 = @"c:\browser\2\";
        const string chromiumLocalDirPath1 = "c:\\browser\\1\\Win64-884014";
        const string chromiumLocalDirPath2 = "c:\\browser\\2\\Win64-884014";

        public StaplesPageRequester(CrawlConfiguration config, IWebContentExtractor contentExtractor, HttpClient httpClient = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _contentExtractor = contentExtractor ?? throw new ArgumentNullException(nameof(contentExtractor));
            if (_config.HttpServicePointConnectionLimit > 0)
                ServicePointManager.DefaultConnectionLimit = _config.HttpServicePointConnectionLimit;
            _httpClient = httpClient;
        }

        //public virtual async Task<CrawledPage> MakeRequestAsync(Uri uri)
        //{
        //    return await MakeRequestAsync(uri, (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        //}

        public virtual async Task<CrawledPage> MakePupRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            CrawledPage crawledPage = new CrawledPage(uri);
            crawledPage.RequestStarted = DateTime.Now;
            countNum++;

            if (httpResponseMessage == null)
            {
                httpResponseMessage = new HttpResponseMessage();
                Uri headerUri = new Uri(@"http://www.google.com");
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (_httpClient == null)
                {
                    _httpClientHandler = BuildHttpClientHandler(uri);
                    _httpClient = BuildHttpClient(_httpClientHandler);
                }
                HttpRequestMessage httpRequestMessage;
                try
                {
                    using (httpRequestMessage = BuildHttpRequestMessage(headerUri))
                    {
                        httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, System.Threading.CancellationToken.None).ConfigureAwait(false);
                    }
                    var statusCode = Convert.ToInt32(httpResponseMessage.StatusCode);
                    if (statusCode < 200 || statusCode > 399)
                        throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
                }
                catch (HttpRequestException hre) { crawledPage.HttpRequestException = hre; }
                catch (TaskCanceledException ex) { crawledPage.HttpRequestException = new HttpRequestException("Request timeout occurred", ex); }
                catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
            }
            if (_httpClientHandler != null)
                crawledPage.HttpClientHandler = _httpClientHandler;
            crawledPage.HttpResponseMessage = httpResponseMessage;
            crawledPage.HttpRequestMessage = BuildHttpRequestMessage(uri);
            crawledPage.RequestCompleted = DateTime.Now;

            if (puppeteerExtra == null)
            {
                puppeteerExtra = new PuppeteerExtra();
                puppeteerExtra.Use(new StealthPlugin());
            }

            if (puppeteerOptions == null)
            {
                puppeteerOptions = new LaunchOptions();
                puppeteerOptions.Browser = SupportedBrowser.Chromium;
                puppeteerOptions.Headless = false;
                puppeteerOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins" };
                puppeteerOptions.LogProcess = false;
                puppeteerOptions.DefaultViewport = null;
            }
            await Chromium();

            puppeteerOptions.ExecutablePath = browserFetcher.GetInstalledBrowsers().First(browser => browser.BuildId == Chrome.DefaultBuildId).GetExecutablePath();

            try
            {
                if (puppeteerBrowser == null)
                    puppeteerBrowser = await puppeteerExtra.LaunchAsync(puppeteerOptions);
            }
            catch (Exception e) { }

            if (puppeteerPage == null)
            {
                try
                {
                    puppeteerPage = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage, crawledPage, shouldDownloadContent);
            }
            if (puppeteerPage2 == null)
            {
                try
                {
                    puppeteerPage2 = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage2, crawledPage, shouldDownloadContent);
            }
            if (puppeteerPage3 == null)
            {
                try
                {
                    puppeteerPage3 = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage3, crawledPage, shouldDownloadContent);
            }
            if (puppeteerPage4 == null)
            {
                try
                {
                    puppeteerPage4 = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage4, crawledPage, shouldDownloadContent);
            }
            if (puppeteerPage5 == null)
            {
                try
                {
                    puppeteerPage5 = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage5, crawledPage, shouldDownloadContent);
            }
            if (puppeteerPage6 == null)
            {
                try
                {
                    puppeteerPage6 = await puppeteerBrowser.NewPageAsync();
                }
                catch (Exception e) { }
                return await CreateChromiumPage(puppeteerPage6, crawledPage, shouldDownloadContent);
            }
            return null;
        }

        private async Task<CrawledPage> CreateChromiumPage(IPage pupPage, CrawledPage thisCrawledPage, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            CrawledPage _result = new CrawledPage(thisCrawledPage.Uri);
            _result = thisCrawledPage;
            HttpResponseMessage pupPageHttpResponseMessage = new HttpResponseMessage();
            pupPageHttpResponseMessage = httpResponseMessage;
            pupPage.DefaultNavigationTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;
            pupPage.DefaultTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;

            try
            {
                using (puppeteerBrowser = await puppeteerExtra.LaunchAsync(puppeteerOptions))
                using (puppeteerPage = await puppeteerBrowser.NewPageAsync())
                {
                    puppeteerResponse = await puppeteerPage.GoToAsync(pupPage.Url);
                    httpResponseMessage.Content = new StringContent(await puppeteerPage.GetContentAsync());
                    httpResponseMessage.StatusCode = puppeteerResponse.Status;
                }
                var statusCode = Convert.ToInt32(puppeteerResponse.Status);
                if (statusCode < 200 || statusCode > 399)
                    throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
            }
            catch (HttpRequestException hre) { _result.HttpRequestException = hre; }
            catch (TaskCanceledException ex) { _result.HttpRequestException = new HttpRequestException("Request timeout occurred", ex); }
            catch (Exception e) { _result.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
            finally
            {
                try
                {
                    if (pupPage != null)
                    {
                        var shouldDownloadContentDecision = shouldDownloadContent(_result);
                        if (shouldDownloadContentDecision.Allow)
                        {
                            _result.DownloadContentStarted = DateTime.Now;
                            _result.Content = await _contentExtractor.GetContentAsync(pupPageHttpResponseMessage).ConfigureAwait(false);
                            _result.DownloadContentCompleted = DateTime.Now;
                        }
                    }
                }
                catch (Exception e) { }
            }
            pupPage.DisposeAsync().ConfigureAwait(false);
            return _result;
        }

        private async Task Chromium()
        {
            if (!System.IO.Directory.Exists(chromiumLocalDirPath1))
            {
                browserFetcherOptions1 = new BrowserFetcherOptions { Path = chromeLocalPath1, Browser = SupportedBrowser.Chromium };
                browserFetcher1 = new BrowserFetcher(browserFetcherOptions1);
                await browserFetcher1.DownloadAsync();
                System.Threading.Thread.Sleep(10000);
            }
            if (countNum > 9 && countNum < 20) try { if (System.IO.Directory.Exists(chromiumLocalDirPath2)) System.IO.Directory.Delete(chromiumLocalDirPath2, true); } catch (Exception e) { }
            if (countNum >= 20 && countNum <= 25)
                if (!System.IO.Directory.Exists(chromiumLocalDirPath2))
                {
                    browserFetcherOptions2 = new BrowserFetcherOptions { Path = chromeLocalPath2, Browser = SupportedBrowser.Chromium };
                    browserFetcher2 = new BrowserFetcher(browserFetcherOptions2);
                    await browserFetcher2.DownloadAsync();
                }
            if (countNum > 59 && countNum < 70)
                try { if (System.IO.Directory.Exists(chromiumLocalDirPath1)) System.IO.Directory.Delete(chromiumLocalDirPath1, true); }
                catch (Exception e) { }
            if (countNum >= 70 && countNum <= 75)
                if (System.IO.Directory.Exists(chromiumLocalDirPath1))
                {
                    browserFetcherOptions1 = new BrowserFetcherOptions { Path = chromeLocalPath1, Browser = SupportedBrowser.Chromium };
                    browserFetcher1 = new BrowserFetcher(browserFetcherOptions1);
                    await browserFetcher1.DownloadAsync();
                }
            if (countNum < 0 && countNum > 100)
            {
                browserFetcherOptions = new BrowserFetcherOptions { Path = chromeLocalPath1, Browser = SupportedBrowser.Chromium };
                browserFetcher = new BrowserFetcher(browserFetcherOptions);
                puppeteerOptions.UserDataDir = chromiumLocalDirPath1;
                countNum = 0;
            }
            else
            {
                if (countNum <= 50)
                {
                    browserFetcherOptions1 = browserFetcherOptions1 ?? new BrowserFetcherOptions { Path = chromeLocalPath1, Browser = SupportedBrowser.Chromium };
                    browserFetcher1 = browserFetcher1 ?? new BrowserFetcher(browserFetcherOptions1);
                    puppeteerOptions.UserDataDir = chromiumLocalDirPath1;
                }
                else
                {
                    if (countNum >= 100) countNum = 0;
                    browserFetcherOptions2 = browserFetcherOptions2 ?? new BrowserFetcherOptions { Path = chromeLocalPath2, Browser = SupportedBrowser.Chromium };
                    browserFetcher2 = browserFetcher2 ?? new BrowserFetcher(browserFetcherOptions2);
                    puppeteerOptions.UserDataDir = chromiumLocalDirPath2;
                }
            }
        }

        public virtual async Task<CrawledPage> MakeRequestAsync(Uri uri)
        {
            return await MakeRequestAsync(uri, (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }

        public virtual async Task<CrawledPage> MakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (_httpClient == null)
            {
                _httpClientHandler = BuildHttpClientHandler(uri);
                _httpClient = BuildHttpClient(_httpClientHandler);
            }

            var crawledPage = new CrawledPage(uri);
            HttpResponseMessage response = null;
            try
            {
                crawledPage.RequestStarted = DateTime.Now;
                using (var requestMessage = BuildHttpRequestMessage(uri))
                {
                    response = await _httpClient.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false);
                }

                var statusCode = Convert.ToInt32(response.StatusCode);
                if (statusCode < 200 || statusCode > 399)
                    throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
            }
            catch (HttpRequestException hre)
            {
                crawledPage.HttpRequestException = hre;
                Log.Debug("Error occurred requesting url [{0}] {@Exception}", uri.AbsoluteUri, hre);
            }
            catch (TaskCanceledException ex)
            {
                crawledPage.HttpRequestException = new HttpRequestException("Request timeout occurred", ex);//https://stackoverflow.com/questions/10547895/how-can-i-tell-when-httpclient-has-timed-out
                Log.Debug("Error occurred requesting url [{0}] {@Exception}", uri.AbsoluteUri, crawledPage.HttpRequestException);
            }
            catch (Exception e)
            {
                crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e);
                Log.Debug("Error occurred requesting url [{0}] {@Exception}", uri.AbsoluteUri, crawledPage.HttpRequestException);
            }
            finally
            {
                crawledPage.HttpRequestMessage = response?.RequestMessage;
                crawledPage.RequestCompleted = DateTime.Now;
                crawledPage.HttpResponseMessage = response;
                crawledPage.HttpClientHandler = _httpClientHandler;
                try
                {
                    if (response != null)
                    {
                        var shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                        if (shouldDownloadContentDecision.Allow)
                        {
                            crawledPage.DownloadContentStarted = DateTime.Now;
                            crawledPage.Content = await _contentExtractor.GetContentAsync(response).ConfigureAwait(false);
                            crawledPage.DownloadContentCompleted = DateTime.Now;
                        }
                        else
                        {
                            Log.Debug("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Error occurred finalizing requesting url [{0}] {@Exception}", uri.AbsoluteUri, e);
                }
            }

            return crawledPage;
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

            if (_config.HttpRequestTimeoutInSeconds > 0)
                httpClient.Timeout = TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds);

            if (_config.IsAlwaysLogin)
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_config.LoginUser + ":" + _config.LoginPassword));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
            }

            return httpClient;
        }

        protected virtual HttpClientHandler BuildHttpClientHandler(Uri rootUri)
        {
            if (rootUri == null)
                throw new ArgumentNullException(nameof(rootUri));

            var httpClientHandler = new HttpClientHandler
            {
                MaxAutomaticRedirections = _config.HttpRequestMaxAutoRedirects,
                UseDefaultCredentials = _config.UseDefaultCredentials
            };

            if (_config.IsHttpRequestAutomaticDecompressionEnabled)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (_config.HttpRequestMaxAutoRedirects > 0)
                httpClientHandler.AllowAutoRedirect = _config.IsHttpRequestAutoRedirectsEnabled;

            if (_config.IsSendingCookiesEnabled)
            {
                httpClientHandler.CookieContainer = _cookieContainer;
                httpClientHandler.UseCookies = true;
            }

            if (!_config.IsSslCertificateValidationEnabled)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            if (_config.IsAlwaysLogin && rootUri != null)
            {
                //Added to handle redirects clearing auth headers which result in 401...
                //https://stackoverflow.com/questions/13159589/how-to-handle-authenticatication-with-httpwebrequest-allowautoredirect
                var cache = new CredentialCache();
                cache.Add(new Uri($"http://{rootUri.Host}"), "Basic", new NetworkCredential(_config.LoginUser, _config.LoginPassword));
                cache.Add(new Uri($"https://{rootUri.Host}"), "Basic", new NetworkCredential(_config.LoginUser, _config.LoginPassword));

                httpClientHandler.Credentials = cache;
            }

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
            puppeteerBrowser.CloseAsync().Wait();
        }
    }
}


//private static HttpContent CreateHttpContent(object content)
//{
//    HttpContent httpContent = null;

//    if (content != null)
//    {
//        var ms = new MemoryStream();
//        SerializeJsonIntoStream(content, ms);
//        ms.Seek(0, SeekOrigin.Begin);
//        httpContent = new StreamContent(ms);
//        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//    }

//    return httpContent;
//}

//public static void SerializeJsonIntoStream(object value, Stream stream)
//{
//    using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
//    using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
//    {
//        var js = new JsonSerializer();
//        js.Serialize(jtw, value);
//        jtw.Flush();
//    }
//}

//string countFileName = "count.txt";
//string workingDirLocalPath = @"c:\browser\";
//string countTxt;
//if (File.Exists(workingDirLocalPath + countFileName))
//{
//    while (countNum == -1)
//    {
//        try
//        {
//            countTxt = await System.IO.File.ReadAllTextAsync(workingDirLocalPath + countFileName);
//        }
//        catch (Exception e)
//        { continue; }
//        if (int.TryParse(countTxt, out countNum))
//            countNum++;
//        else
//            countNum = 1;
//    }
//}

//countNum++;
//try
//{
//   await System.IO.File.WriteAllTextAsync(workingDirLocalPath + countFileName, countNum.ToString());
//}
//catch (Exception e) { }