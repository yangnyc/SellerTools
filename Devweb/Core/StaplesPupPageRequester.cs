using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Devweb.Core
{
    public interface IStaplesPupPageRequester : IDisposable
    {
        Task<CrawledPage> MakeRequestAsync(Uri uri);
        Task<CrawledPage> MakeRequestAsync(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
    }

    public class StaplesPupPageRequester : IStaplesPupPageRequester
    {
        private readonly CrawlConfiguration _config;
        private readonly IWebContentExtractor _contentExtractor;
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private HttpClientHandler _httpClientHandler;
        private HttpClient _httpClient;
        HttpResponseMessage httpResponseMessage, httpResponseMessage1, httpResponseMessage2;
        CrawledPage crawledPage, crawledPage1, crawledPage2;
        int countNum;
        BrowserFetcherOptions browserFetcherOptions1;
        BrowserFetcher browserFetcher1;
        BrowserFetcherOptions browserFetcherOptions2;
        BrowserFetcher browserFetcher2;
        BrowserFetcherOptions browserFetcherOptions;
        BrowserFetcher browserFetcher;
        PuppeteerExtra pupExtra;
        IPage pupPage1;
        IPage pupPage2;
        LaunchOptions pupOptions;
        ConnectOptions pupConnectOptions;
        IBrowser pupBrowser;
        const string chromeLocalPath1 = @"c:\browser\1\";
        const string chromeLocalPath2 = @"c:\browser\2\";
        const string chromiumLocalDirPath1 = "c:\\browser\\1\\Win64-884014";
        const string chromiumLocalDirPath2 = "c:\\browser\\2\\Win64-884014";

        public StaplesPupPageRequester(CrawlConfiguration config, IWebContentExtractor contentExtractor, HttpClient httpClient = null)
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
                Uri headerUri = new Uri(@"http://www.staplesadvantage.com");
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
                catch (Exception e) { crawledPage.HttpRequestException = new HttpRequestException("Unknown error occurred", e); }
            }
            if (_httpClientHandler != null)
                crawledPage.HttpClientHandler = _httpClientHandler;
            crawledPage.HttpResponseMessage = httpResponseMessage;
            crawledPage.HttpRequestMessage = BuildHttpRequestMessage(uri);
            crawledPage.RequestCompleted = DateTime.Now;

            if (pupExtra == null) { pupExtra = new PuppeteerExtra(); pupExtra.Use(new StealthPlugin()); }

            if (pupOptions == null)
            {
                pupOptions = new LaunchOptions();
                pupOptions.Browser = SupportedBrowser.Chromium;
                pupOptions.Headless = false;
                pupOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins", "--remote-debugging-port=2122", "--blink-settings=imagesEnabled=false" };
                pupOptions.LogProcess = false;
                pupOptions.DefaultViewport = null;
            }
            await Chromium();
            pupOptions.ExecutablePath = browserFetcher.GetInstalledBrowsers().First(browser => browser.BuildId == PuppeteerSharp.BrowserData.Chrome.DefaultBuildId).GetExecutablePath();

            if (pupConnectOptions == null)
            {
                pupConnectOptions = new ConnectOptions();
                pupConnectOptions.BrowserURL = "http://127.0.0.1:2122";
                pupConnectOptions.DefaultViewport = null;
            }
            try
            {
                if (pupBrowser == null)
                    try
                    {
                        pupBrowser = await pupExtra.ConnectAsync(pupConnectOptions);
                        pupBrowser = await pupExtra.LaunchAsync(pupOptions);
                    }
                    catch (Exception e) { pupExtra = new PuppeteerExtra(); pupExtra.Use(new StealthPlugin()); }
                if (pupBrowser == null)
                    pupBrowser = await pupExtra.LaunchAsync(pupOptions);
            }
            catch (Exception e) { return null; }

            //Page 1
            if (pupPage1 == null)
                using (pupPage1 = await pupBrowser.NewPageAsync())
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
                using (pupPage2 = await pupBrowser.NewPageAsync())
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
                pupOptions.UserDataDir = chromiumLocalDirPath1;
                countNum = 0;
            }
            else
            {
                if (countNum <= 50)
                {
                    browserFetcherOptions1 = browserFetcherOptions1 ?? new BrowserFetcherOptions { Path = chromeLocalPath1, Browser = SupportedBrowser.Chromium };
                    browserFetcher1 = browserFetcher1 ?? new BrowserFetcher(browserFetcherOptions1);
                    pupOptions.UserDataDir = chromiumLocalDirPath1;
                }
                else
                {
                    if (countNum >= 100) countNum = 0;
                    browserFetcherOptions2 = browserFetcherOptions2 ?? new BrowserFetcherOptions { Path = chromeLocalPath2, Browser = SupportedBrowser.Chromium };
                    browserFetcher2 = browserFetcher2 ?? new BrowserFetcher(browserFetcherOptions2);
                    pupOptions.UserDataDir = chromiumLocalDirPath2;
                }
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
            pupBrowser.CloseAsync().Wait();
        }
    }
}

//await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
//await pupPage1.WaitForTimeoutAsync(2000);
//await pupPage1.EvaluateExpressionAsync("if(typeof bL !== 'undefined'){delete(bL);}");
//await pupPage1.EvaluateExpressionAsync("if(typeof bE !== 'undefined'){delete(bE);}");
//await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('loadMoreButton')");
//await pupPage1.EvaluateExpressionAsync("bE = document.body, bL");
//await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
//await pupPage1.WaitForTimeoutAsync(2000);
//await pupPage1.EvaluateExpressionAsync("if(typeof bL !== 'undefined'){delete(bL);}");
//await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('loadMoreButton')");
//while (await pupPage1.EvaluateFunctionAsync<bool>("(aOut)=>document.getElementById('loadMoreButton') != null"))
//{
//    await pupPage1.EvaluateExpressionAsync("bL.click()");
//                           await Task.Delay(2000);
//    await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
//                           await Task.Delay(2000);
//    await pupPage1.EvaluateExpressionAsync("if(typeof bL !== 'undefined'){delete(bL);}");
//    await pupPage1.EvaluateExpressionAsync("if(typeof bE !== 'undefined'){delete(bE);}");
//    await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('loadMoreButton')");
//    await pupPage1.EvaluateExpressionAsync("bE = document.body, bL");
//    await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:bE.offsetTop - bE.offsetHeight,behavior:'smooth'})");
//                           await Task.Delay(2000);
//    await pupPage1.EvaluateExpressionAsync("if(typeof bL !== 'undefined'){delete(bL);}");
//    await pupPage1.EvaluateExpressionAsync("bL = document.getElementById('loadMoreButton')");
//                           await Task.Delay(2000);
//    await pupPage1.EvaluateExpressionAsync("window.scrollTo({top:document.body.scrollHeight,behavior:'smooth'})");
//                           await Task.Delay(2000);
//}








//try
//{
//    if (puppeteerBrowser == null)
//    {
//        using (puppeteerBrowser = await puppeteerExtra.LaunchAsync(puppeteerOptions))
//        using (puppeteerPage = await puppeteerBrowser.NewPageAsync())
//        {
//            puppeteerResponse = await puppeteerPage.GoToAsync(uri.AbsoluteUri);
//            puppeteerPage = await puppeteerBrowser.NewPageAsync();
//            puppeteerResponse = await puppeteerPage.GoToAsync(uri.AbsoluteUri);

//            httpResponseMessage.Content = new StringContent(await puppeteerPage.GetContentAsync());
//            httpResponseMessage.StatusCode = puppeteerResponse.Status;
//        }
//    }
//    else
//    {
//        puppeteerResponse = await puppeteerPage.GoToAsync(uri.AbsoluteUri);

//        httpResponseMessage.Content = new StringContent(await puppeteerPage.GetContentAsync());
//        httpResponseMessage.StatusCode = puppeteerResponse.Status;
//    }
//    var statusCode = Convert.ToInt32(puppeteerResponse.Status);
//    if (statusCode < 200 || statusCode > 399)
//        throw new HttpRequestException($"Server response was unsuccessful, returned [http {statusCode}]");
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


//if (puppeteerPage == null)
//{
//    try
//    {
//        puppeteerPage = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//}
//if (puppeteerPage2 == null)
//{
//    try
//    {
//        puppeteerPage2 = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//    return await CreateChromiumPage(puppeteerPage2, crawledPage, shouldDownloadContent);
//}
//if (puppeteerPage3 == null)
//{
//    try
//    {
//        puppeteerPage3 = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//    return await CreateChromiumPage(puppeteerPage3, crawledPage, shouldDownloadContent);
//}
//if (puppeteerPage4 == null)
//{
//    try
//    {
//        puppeteerPage4 = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//    return await CreateChromiumPage(puppeteerPage4, crawledPage, shouldDownloadContent);
//}
//if (puppeteerPage5 == null)
//{
//    try
//    {
//        puppeteerPage5 = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//    return await CreateChromiumPage(puppeteerPage5, crawledPage, shouldDownloadContent);
//}
//if (puppeteerPage6 == null)
//{
//    try
//    {
//        puppeteerPage6 = await puppeteerBrowser.NewPageAsync();
//    }
//    catch (Exception e) { }
//    return await CreateChromiumPage(puppeteerPage6, crawledPage, shouldDownloadContent);
//}