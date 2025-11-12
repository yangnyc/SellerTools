using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Poco;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApp.Code.Crawler.CVS
{
    /// <summary>
    /// Puppeteer-based web crawler for CVS pharmacy website.
    /// Provides methods to extract HTML and AngleSharp documents from URLs.
    /// </summary>
    public class CVSPupCrawl
    {
        static PupPageRequester? pupPageRequester;
        const string chromeLocalPath1 = @"c:\browser\1\";
        const string chromiumLocalDirPath1 = "c:\\browser\\1\\Win64-884014";

        /// <summary>
        /// Retrieves raw HTML content from the specified URL.
        /// </summary>
        /// <param name="urlToCrawl">The URL to crawl.</param>
        /// <returns>HTML string, or null if crawling failed.</returns>
        public async Task<string?> GetUrlHtml(string urlToCrawl)
        {
            if (urlToCrawl == null)
                return null;
            CrawledPage? crawledPage = await GetCrawledPage(urlToCrawl);
            if (crawledPage == null)
                return null;
            return crawledPage.AngleSharpHtmlDocument.ToHtml();
        }

        /// <summary>
        /// Retrieves an AngleSharp HTML document from the specified URL.
        /// </summary>
        /// <param name="urlToCrawl">The URL to crawl.</param>
        /// <returns>AngleSharp IHtmlDocument, or null if crawling failed.</returns>
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


        /*public async Task<bool> PrcsStplsProdPerSupCatgs()
        {
            CVSCatgs CVSCatgs;
            CVSCatgs CVSCatgsTemp = null;
            CVSCatgsFunc CVSCatgsFunc = new();
            CrawledPage crawledPage;

            CVSCatgs = CVSCatgsFunc.getNextIsCollectedHRefFalse();
            while (CVSCatgs != null)
            {
                if (CVSCatgsTemp != null && CVSCatgsTemp.Id == CVSCatgs.Id) { System.Console.Out.WriteLine("Same CatId received by getNextIsCollectedHRefFalse:" + CVSCatgs.Url); return false; }
                CVSCatgsTemp = CVSCatgs;

                crawledPage = await GetHtml(CVSCatgs.Url);
                if (!PrcsRowsPerPage(crawledPage)) return false;
                CVSCatgsFunc.SetIsCollectedHRefToTrue(CVSCatgs.Id);
                CVSCatgs = CVSCatgsFunc.getNextIsCollectedHRefFalse();
            }
            return true;
        }*/

        /*private bool PrcsRowsPerPage(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CVSCatgsFunc CVSCatgsFunc = new();
            CVSProdsFunc CVSProdsFunc = new();
            CVSProducts CVSProducts = null;
            string price, _link;
            IHtmlDivElement divTemp;
            IHtmlDivElement divDowTable = angleSharpHtmlDocument.GetElementsByClassName("grid__row").Any() ? (IHtmlDivElement)angleSharpHtmlDocument.GetElementsByClassName("grid__row")[1] : null;
            if (divDowTable == null)
            {
                System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " had no products.");
                return true;
            }
            foreach (IHtmlDivElement htmlDivElement in divDowTable.Children)
            {
                _link = ((IHtmlAnchorElement)htmlDivElement.QuerySelector("a")).Href.Trim().Replace(@"about://", @"https://www.CVS.com");
                if (_link != null)
                    CVSProducts = CVSProdsFunc.getByUrl(_link);
                if (CVSProducts == null)
                {
                    CVSProducts = new();
                    CVSProducts.Url = _link;
                    CVSProdsFunc.Add(CVSProducts);
                }

                CVSProducts.ImgMain = htmlDivElement.QuerySelectorAll("img").Any() ? ((IHtmlImageElement)htmlDivElement.QuerySelectorAll("img").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__image")).FirstOrDefault()).Source.Trim() : null;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__title")).FirstOrDefault();
                if (divTemp != null) CVSProducts.Title = divTemp.QuerySelector("a").TextContent;
                price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__final_price false")).FirstOrDefault().TextContent;
                if (!string.IsNullOrEmpty(price))
                    if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out double priceNum))
                        CVSProducts.PriceBuyDefCVS = priceNum;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__price_per_unit")).FirstOrDefault();
                if (divTemp != null) { if (divTemp.FirstChild != null) CVSProducts.UnitOfMeas = divTemp.FirstChild.TextContent; }

                CVSProducts.Model = CVSProducts.Url[(CVSProducts.Url.IndexOf("product_") + 8)..].Trim();
                CVSProducts.IsAllBackOrdered = false;
                CVSProducts.DateLastAvail = DateTime.Now.ToString();
                CVSProducts.IsCollectedFull = true;
                CVSProducts.IsActive = true;
                CVSProdsFunc.Set(CVSProducts);
            }
            return true;
        }
*/

        /*private bool CollDataStplsProdPerSC(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CVSCatgsFunc CVSCatgsFunc = new();
            CVSProdsFunc CVSProdsFunc = new();
            CVSProducts CVSProducts = null;
            string price, _link, sourceSet;
            IHtmlDivElement divTemp;
            IHtmlDivElement divDowTable = angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery") == null ? null : angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery").GetElementsByClassName("grid__row").Any() ? (IHtmlDivElement)angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery").GetElementsByClassName("grid__row")[0] : null;
            if (divDowTable == null)
            {
                System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " had no products.");
                return true;
            }
            foreach (IHtmlDivElement htmlDivElement in divDowTable.Children)
            {
                _link = ((IHtmlAnchorElement)htmlDivElement.QuerySelector("a")).Href.Trim().Replace(@"about://", @"https://www.CVS.com");
                if (_link != null)
                    CVSProducts = CVSProdsFunc.getByUrl(_link);
                if (CVSProducts == null)
                {
                    CVSProducts = new();
                    CVSProducts.Url = _link;
                    CVSProdsFunc.Add(CVSProducts);
                }

                sourceSet = htmlDivElement.QuerySelectorAll("img").Any() ? ((IHtmlImageElement)htmlDivElement.QuerySelectorAll("img").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__image picture__img_resp")).FirstOrDefault()).SourceSet : null;
                CVSProducts.ImgMain = sourceSet.Split(' ').Where(x => x.Contains("www.CVS")).FirstOrDefault();
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__title")).FirstOrDefault();
                if (divTemp != null) CVSProducts.Title = divTemp.QuerySelector("a").TextContent;
                price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__final_price false")).FirstOrDefault().TextContent;
                if (!string.IsNullOrEmpty(price))
                    if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out double priceNum))
                        CVSProducts.PriceBuyDefCVS = priceNum;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__price_per_unit")).FirstOrDefault();
                if (divTemp != null) { if (divTemp.FirstChild != null) CVSProducts.UnitOfMeas = divTemp.FirstChild.TextContent; }

                CVSProducts.Model = CVSProducts.Url[(CVSProducts.Url.IndexOf("product_") + 8)..].Trim();
                CVSProducts.IsAllBackOrdered = false;
                CVSProducts.DateLastAvail = DateTime.Now.ToString();
                CVSProducts.IsCollectedFull = true;
                CVSProducts.IsActive = true;
                CVSProdsFunc.Set(CVSProducts);
            }
            return true;
        }
*/

        /*public async Task<bool> GetTitlePriceByCVSModelNum()
        {
            CVSProdsFunc CVSProdsFunc = new();
            CVSProducts CVSProducts;
            CVSProducts tempCVSProducts = null;

            Page pupPage;
            puppeteerExtra = new PuppeteerExtra();
            puppeteerExtra.Use(new StealthPlugin());

            puppeteerOptions = new();
            puppeteerOptions.Product = Product.Chrome;
            puppeteerOptions.Headless = false;
            puppeteerOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins" };
            puppeteerOptions.LogProcess = false;
            puppeteerOptions.DefaultViewport = null;
            browserFetcherOptions = new BrowserFetcherOptions
            {
                Path = chromeLocalPath1,
                Product = Product.Chrome
            };
            browserFetcher = new BrowserFetcher(browserFetcherOptions);
            if (!System.IO.Directory.Exists(chromiumLocalDirPath1))
            {
                await browserFetcher.DownloadAsync();
                System.Threading.Thread.Sleep(10000);
            }
            else
                revisionInfo = await browserFetcher.GetRevisionInfoAsync();
            puppeteerOptions.UserDataDir = chromiumLocalDirPath1;
            puppeteerOptions.ExecutablePath = revisionInfo.ExecutablePath;

            try
            {
                puppeteerBrowser = await puppeteerExtra.LaunchAsync(puppeteerOptions);
                pupPage = await puppeteerBrowser.NewPageAsync();
                pupPage.DefaultNavigationTimeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;
                pupPage.DefaultTimeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;

                Response pupResponse = await pupPage.GoToAsync(@"https://www.CVS.com/Office-Supplies/cat_SC1");
                string price, url;
                CVSProducts = CVSProdsFunc.getNextIsCollectedFullFalse();
                while (CVSProducts != null)
                {
                    if (tempCVSProducts != null)
                        if (CVSProducts.Id == tempCVSProducts.Id)
                            return false;
                    tempCVSProducts = CVSProducts;

                    string partNum = CVSProducts.Url[(CVSProducts.Url.IndexOf("product_") + 8)..].Trim();
                    await pupPage.EvaluateExpressionAsync("document.querySelector('#searchInput').value = ''");
                    await pupPage.TypeAsync("#searchInput", partNum);
                    await pupPage.WaitForTimeoutAsync(1000);
                    if (await pupPage.EvaluateFunctionAsync<bool>("(aOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0] == null"))
                    {
                        CVSProducts.IsCollectedFull = true;
                        CVSProducts.IsActive = false;
                        CVSProducts.IsAllBackOrdered = true;
                        CVSProducts.Model = partNum;
                        CVSProdsFunc.Set(CVSProducts);
                        CVSProducts = CVSProdsFunc.getNextIsCollectedFullFalse();
                        continue;
                    }
                    url = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0].href");
                    if (!CVSProducts.Url.Trim().ToLower().Equals(url.ToLower()))
                    {
                        // if(CVSProdsFunc.)
                        CVSProducts.IsCollectedFull = true;
                        CVSProdsFunc.Set(CVSProducts);

                        CVSProducts = CVSProdsFunc.getNextIsCollectedFullFalse();
                        continue;
                        CVSProducts.Title = "WRONG URL IN CVS.COM MANNUAL CHECK NEEDED!" + url;
                        CVSProducts.IsCollectedFull = true;
                        CVSProducts.Model = partNum;
                        CVSProdsFunc.Set(CVSProducts);
                        CVSProducts = new();
                        CVSProducts.Model = partNum;
                        CVSProducts.Url = partNum;
                        CVSProdsFunc.Add(CVSProducts);
                    }
                    CVSProducts.ImgMain = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_image')[0].src");
                    price = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__final_price')[0].textContent");
                    if (!string.IsNullOrEmpty(price))
                        if (double.TryParse(price.Replace("$", "").Trim().ToCharArray(), out double priceNum))
                            CVSProducts.PriceBuyDefCVS = priceNum;
                    CVSProducts.Title = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0].innerText");
                    CVSProducts.IsActive = true;
                    CVSProducts.IsAllBackOrdered = false;
                    CVSProducts.DateLastAvail = DateTime.Now.ToString();
                    CVSProducts.PriceBuyCurrentCVS = CVSProducts.PriceBuyDefCVS;
                    CVSProducts.IsCollectedFull = true;
                    CVSProdsFunc.Set(CVSProducts);
                    CVSProducts = CVSProdsFunc.getNextIsCollectedFullFalse();
                }
            }
            catch (Exception e) { System.Console.Out.WriteLine(e.Message); return false; }
            return true;
        }
*/

        /// <summary>
        /// Creates a crawled page from the specified URI.
        /// Configures web crawler settings including timeouts and user agent.
        /// </summary>
        /// <param name="_uri">The URI to crawl.</param>
        /// <returns>A CrawledPage containing the response and parsed HTML.</returns>
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

            if (pupPageRequester == null)
                pupPageRequester = new(crawlConfiguration, new WebContentExtractor(), null);
            return await pupPageRequester.MakeRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }
    }
}
