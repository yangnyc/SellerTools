using AngleSharp.Dom;
using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
        HttpResponseMessage httpResponseMessage, httpResponseMessage1, httpResponseMessage2;
        CrawledPage crawledPage, crawledPage1, crawledPage2;
        int countNum;
        BrowserFetcherOptions browserFetcherOptions;
        BrowserFetcher browserFetcher;
        PuppeteerExtra pupExtra;
        IBrowser browserLocal;
        IPage pupPage1;
        IPage pupPage2;
        LaunchOptions launchOptions;
        const string chromiumLocalDirPath1 = @"c:\browser\1\";
        const string chromiumLocalDirPath2 = @"c:\browser\2\";
        const string chromiumZippedFileName = @"c:\browser\Chromium\Chromium.zip";

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
            crawledPage = new CrawledPage(uri);
            crawledPage.RequestStarted = DateTime.Now;
            countNum++;

            if (httpResponseMessage == null)
            {
                httpResponseMessage = new HttpResponseMessage();
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                Uri headerUri = new Uri($"{uri.Scheme}://{uri.Authority}");
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
                catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
            }
            if (_httpClientHandler != null)
                crawledPage.HttpClientHandler = _httpClientHandler;
            crawledPage.HttpResponseMessage = httpResponseMessage;
            crawledPage.HttpRequestMessage = BuildHttpRequestMessage(uri);
            crawledPage.RequestCompleted = DateTime.Now;

            if (pupExtra == null) { pupExtra = new PuppeteerExtra(); pupExtra.Use(new StealthPlugin()); }
            BuildLaunchOptions();

            browserLocal = await InstallBrowserLocal(chromiumLocalDirPath1);

            //Page 1
            if (pupPage1 == null)
                using (pupPage1 = await browserLocal.NewPageAsync())
                {
                    crawledPage1 = crawledPage; httpResponseMessage1 = httpResponseMessage;
                    try
                    {
                        pupPage1.DefaultNavigationTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;
                        pupPage1.DefaultTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;
                        IResponse pupResponse1 = await pupPage1.GoToAsync(crawledPage1.Uri.AbsoluteUri);
                        await Task.Delay(2000);
                        await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
                        await Task.Delay(2000);
                        await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('footerContainer')");
                        await pupPage1.EvaluateExpressionAsync("bE = document.body, bL");
                        await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
                        await Task.Delay(3000);
                        pupResponse1 = await pupPage1.GoToAsync(crawledPage1.Uri.AbsoluteUri);
                        await Task.Delay(2000);
                        await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
                        await Task.Delay(2000);
                        await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('footerContainer')");
                        await pupPage1.EvaluateExpressionAsync("bE = document.body, bL");
                        await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
                        await Task.Delay(2000);

                        httpResponseMessage1.Content = new StringContent(await pupPage1.GetContentAsync());
                        httpResponseMessage1.StatusCode = pupResponse1.Status;

                        var statusCode = Convert.ToInt32(pupResponse1.Status);
                        if (statusCode < 200 || statusCode > 399) throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
                    }
                    catch (Exception e) { crawledPage1.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
                    finally
                    {
                        try
                        {
                            if (pupPage1 != null)
                            {
                                var shouldDownloadContentDecision = shouldDownloadContent(crawledPage1);
                                if (shouldDownloadContentDecision.Allow)
                                {
                                    crawledPage1.DownloadContentStarted = DateTime.Now;
                                    crawledPage1.Content = await _contentExtractor.GetContentAsync(httpResponseMessage1).ConfigureAwait(false);
                                    crawledPage1.DownloadContentCompleted = DateTime.Now;
                                }
                            }
                        }
                        catch (Exception e) { }
                    }
                    return crawledPage1;
                }
            else
                using (pupPage2 = await browserLocal.NewPageAsync())
                {
                    crawledPage2 = crawledPage; httpResponseMessage2 = httpResponseMessage;
                    try
                    {
                        pupPage2.DefaultNavigationTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;
                        pupPage2.DefaultTimeout = (int)TimeSpan.FromSeconds(_config.HttpRequestTimeoutInSeconds).TotalMilliseconds;
                        IResponse pupResponse2 = await pupPage2.GoToAsync(crawledPage2.Uri.AbsoluteUri);
                        await Task.Delay(2000);
                        await pupPage2.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
                        await Task.Delay(2000);
                        await pupPage2.EvaluateExpressionAsync("bL = document.getElementById('footerContainer')");
                        await pupPage2.EvaluateExpressionAsync("bE = document.body, bL");
                        await pupPage2.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
                        pupResponse2 = await pupPage2.GoToAsync(crawledPage2.Uri.AbsoluteUri);
                        await Task.Delay(2000);
                        await pupPage2.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
                        await Task.Delay(2000);
                        await pupPage2.EvaluateExpressionAsync("bL = document.getElementById('footerContainer')");
                        await pupPage2.EvaluateExpressionAsync("bE = document.body, bL");
                        await pupPage2.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
                        await Task.Delay(5000);


                        httpResponseMessage2.Content = new StringContent(await pupPage2.GetContentAsync());
                        httpResponseMessage2.StatusCode = pupResponse2.Status;

                        var statusCode = Convert.ToInt32(pupResponse2.Status);
                        if (statusCode < 200 || statusCode > 399) throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
                    }
                    catch (Exception e) { crawledPage2.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
                    finally
                    {
                        try
                        {
                            if (pupPage2 != null)
                            {
                                var shouldDownloadContentDecision = shouldDownloadContent(crawledPage2);
                                if (shouldDownloadContentDecision.Allow)
                                {
                                    crawledPage2.DownloadContentStarted = DateTime.Now;
                                    crawledPage2.Content = await _contentExtractor.GetContentAsync(httpResponseMessage2).ConfigureAwait(false);
                                    crawledPage2.DownloadContentCompleted = DateTime.Now;
                                }
                            }
                        }
                        catch (Exception e) { }
                    }
                    return crawledPage2;
                }
            return null;
        }

        protected virtual async Task<IBrowser> InstallBrowserLocal(string path)
        {
            string executablePathLocal = path;
            //Check for chromium zipped
            if (System.IO.File.Exists(chromiumZippedFileName))
            {
                try
                {
                    //Delete old Chromium
                    if (System.IO.Directory.Exists(executablePathLocal))
                        System.IO.Directory.Delete(executablePathLocal, true);
                    executablePathLocal += @"FromZip\";
                    ZipFile.ExtractToDirectory(chromiumZippedFileName, executablePathLocal);
                    executablePathLocal += @"Chromium\chrome.exe";
                }
                catch (Exception ex) { return null; }
            }
            else
            {
                browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = path, Browser = SupportedBrowser.Chromium });
                await browserFetcher.DownloadAsync();
                System.Threading.Thread.Sleep(10000);
                executablePathLocal = browserFetcher.DownloadAsync().Result.GetExecutablePath();
            }
            if (launchOptions == null)
                launchOptions = new LaunchOptions { Headless = true, ExecutablePath = executablePathLocal };
            else
            {
                launchOptions.Headless = true;
                launchOptions.ExecutablePath = executablePathLocal;
            }
            launchOptions.UserDataDir = System.IO.Path.GetDirectoryName(executablePathLocal);
            return await Puppeteer.LaunchAsync(launchOptions);
        }

        protected virtual void BuildLaunchOptions()
        {
            if (launchOptions == null)
            {
                launchOptions = new LaunchOptions();
                launchOptions.Browser = SupportedBrowser.Chromium;
                launchOptions.Headless = false;
                launchOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins", "--remote-debugging-port=2122", "--blink-settings=imagesEnabled=false" };
                launchOptions.LogProcess = false;
                launchOptions.DefaultViewport = null;
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