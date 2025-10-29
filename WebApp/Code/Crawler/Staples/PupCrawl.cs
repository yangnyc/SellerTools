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

namespace WebApp.Code.Crawler.Staples
{
    public class PupCrawl
    {
        static StaplesPupPageRequester? staplesPupPageRequester;
        PuppeteerExtra? puppeteerExtra;
        BrowserFetcherOptions? browserFetcherOptions;
        IBrowserFetcher? browserFetcher;
        LaunchOptions? puppeteerOptions;
        IBrowser? puppeteerBrowser;
        const string chromeLocalPath1 = @"c:\browser\1\";
        const string chromiumLocalDirPath1 = "c:\\browser\\1\\Win64-884014";

        public async Task<bool> PrcsStplsProdPerSupCatgs()
        {
            DataItemCatg? dataItemCatg;
            DataItemCatg? dataItemCatgTemp = null;
            CatgsFunc? catgsFunc = new();
            CrawledPage? crawledPage;

            dataItemCatg = catgsFunc.GetNextIsCollectedHRefFalse();
            while (dataItemCatg != null)
            {
                if (dataItemCatgTemp != null && dataItemCatgTemp.Id == dataItemCatg.Id) { System.Console.Out.WriteLine("Same CatId received by getNextIsCollectedHRefFalse:" + dataItemCatg.Url); return false; }
                dataItemCatgTemp = dataItemCatg;

                crawledPage = await GetHtml(dataItemCatg.Url);
                if (!PrcsRowsPerPage(crawledPage)) return false;
                catgsFunc.SetIsCollectedHRefToTrue(dataItemCatg.Id);
                dataItemCatg = catgsFunc.GetNextIsCollectedHRefFalse();
            }
            return true;
        }
        private static bool PrcsRowsPerPage(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CatgsFunc catgsFunc = new();
            ProdsFunc staplesProdsFunc = new();
            DataItemProduct? dataItemProduct = null;
            string price, _link;
            IHtmlDivElement divTemp;
            IHtmlDivElement? divDowTable = angleSharpHtmlDocument.GetElementsByClassName("grid__row").Any() ? (IHtmlDivElement)angleSharpHtmlDocument.GetElementsByClassName("grid__row")[1] : null;
            if (divDowTable == null)
            {
                System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " had no products.");
                return true;
            }
            foreach (IHtmlDivElement htmlDivElement in divDowTable.Children)
            {
                _link = ((IHtmlAnchorElement)htmlDivElement.QuerySelector("a")).Href.Trim().Replace(@"about://", @"https://www.staples.com");
                if (_link != null)
                    dataItemProduct = staplesProdsFunc.getByUrl(_link);
                if (dataItemProduct == null)
                {
                    dataItemProduct = new();
                    dataItemProduct.Url = _link;
                    staplesProdsFunc.Add(dataItemProduct);
                }

                dataItemProduct.ImgMain = htmlDivElement.QuerySelectorAll("img").Any() ? ((IHtmlImageElement)htmlDivElement.QuerySelectorAll("img").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__image")).FirstOrDefault()).Source.Trim() : null;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__title")).FirstOrDefault();
                if (divTemp != null) dataItemProduct.Title = divTemp.QuerySelector("a").TextContent;
                price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__final_price false")).FirstOrDefault().TextContent;
                if (!string.IsNullOrEmpty(price))
                    if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out double priceNum))
                        dataItemProduct.PriceBuyDef = priceNum;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__price_per_unit")).FirstOrDefault();
                if (divTemp != null) { if (divTemp.FirstChild != null) dataItemProduct.UnitOfMeas = divTemp.FirstChild.TextContent; }

                dataItemProduct.Model = dataItemProduct.Url[(dataItemProduct.Url.IndexOf("product_") + 8)..].Trim();
                dataItemProduct.IsAllBackOrdered = false;
                dataItemProduct.DateLastAvail = DateTime.Now.ToString();
                dataItemProduct.IsCollectedFull = true;
                dataItemProduct.IsActive = true;
                staplesProdsFunc.Set(dataItemProduct);
            }
            return true;
        }

        private bool CollDataStplsProdPerSC(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CatgsFunc catgsFunc = new();
            ProdsFunc prodsFunc = new();
            DataItemProduct? dataItemProduct = null;
            string price, _link, sourceSet;
            IHtmlDivElement divTemp;
            IHtmlDivElement? divDowTable = angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery") == null ? null : angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery").GetElementsByClassName("grid__row").Any() ? (IHtmlDivElement)angleSharpHtmlDocument.GetElementById("dotcomDD4SkuDiscovery").GetElementsByClassName("grid__row")[0] : null;
            if (divDowTable == null)
            {
                System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " had no products.");
                return true;
            }
            foreach (IHtmlDivElement htmlDivElement in divDowTable.Children)
            {
                _link = ((IHtmlAnchorElement)htmlDivElement.QuerySelector("a")).Href.Trim().Replace(@"about://", @"https://www.staples.com");
                if (_link != null)
                    dataItemProduct = prodsFunc.getByUrl(_link);
                if (dataItemProduct == null)
                {
                    dataItemProduct = new();
                    dataItemProduct.Url = _link;
                    prodsFunc.Add(dataItemProduct);
                }

                sourceSet = htmlDivElement.QuerySelectorAll("img").Any() ? ((IHtmlImageElement)htmlDivElement.QuerySelectorAll("img").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__image picture__img_resp")).FirstOrDefault()).SourceSet : null;
                dataItemProduct.ImgMain = sourceSet.Split(' ').Where(x => x.Contains("www.staples")).FirstOrDefault();
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__title")).FirstOrDefault();
                if (divTemp != null) dataItemProduct.Title = divTemp.QuerySelector("a").TextContent;
                price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__final_price false")).FirstOrDefault().TextContent;
                if (!string.IsNullOrEmpty(price))
                    if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out double priceNum))
                        dataItemProduct.PriceBuyDef = priceNum;
                divTemp = (IHtmlDivElement)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__price_per_unit")).FirstOrDefault();
                if (divTemp != null) { if (divTemp.FirstChild != null) dataItemProduct.UnitOfMeas = divTemp.FirstChild.TextContent; }

                dataItemProduct.Model = dataItemProduct.Url[(dataItemProduct.Url.IndexOf("product_") + 8)..].Trim();
                dataItemProduct.IsAllBackOrdered = false;
                dataItemProduct.DateLastAvail = DateTime.Now.ToString();
                dataItemProduct.IsCollectedFull = true;
                dataItemProduct.IsActive = true;
                prodsFunc.Set(dataItemProduct);
            }
            return true;
        }

        public async Task<bool> GetTitlePriceByStaplesModelNum()
        {
            ProdsFunc staplesProdsFunc = new();
            DataItemProduct dataItemProduct;
            DataItemProduct dataItemProductTemp = null;

            IPage pupPage;
            puppeteerExtra = new PuppeteerExtra();
            puppeteerExtra.Use(new StealthPlugin());

            puppeteerOptions = new();
            puppeteerOptions.Browser = SupportedBrowser.Chromium;
            puppeteerOptions.Headless = false;
            puppeteerOptions.Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--site-per-process", "--disable-features=IsolateOrigins" };
            puppeteerOptions.LogProcess = false;
            puppeteerOptions.DefaultViewport = null;
            browserFetcherOptions = new BrowserFetcherOptions
            {
                Path = chromeLocalPath1,
                Browser = SupportedBrowser.Chromium
            };
            browserFetcher = new BrowserFetcher(browserFetcherOptions);
            if (!System.IO.Directory.Exists(chromiumLocalDirPath1))
            {
                await browserFetcher.DownloadAsync();
                System.Threading.Thread.Sleep(10000);
            }
            puppeteerOptions.UserDataDir = chromiumLocalDirPath1;
            puppeteerOptions.ExecutablePath = browserFetcher.GetInstalledBrowsers().First(browser => browser.BuildId == Chrome.DefaultBuildId).GetExecutablePath();

            try
            {
                puppeteerBrowser = await puppeteerExtra.LaunchAsync(puppeteerOptions);
                pupPage = await puppeteerBrowser.NewPageAsync();
                pupPage.DefaultNavigationTimeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;
                pupPage.DefaultTimeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;

                IResponse pupResponse = await pupPage.GoToAsync(@"https://www.staples.com/Office-Supplies/cat_SC1");
                string price, url;
                dataItemProduct = staplesProdsFunc.getNextIsCollectedFullFalse();
                while (dataItemProduct != null)
                {
                    if (dataItemProductTemp != null)
                        if (dataItemProduct.Id == dataItemProductTemp.Id)
                            return false;
                    dataItemProductTemp = dataItemProduct;

                    string partNum = dataItemProduct.Url[(dataItemProduct.Url.IndexOf("product_") + 8)..].Trim();
                    await pupPage.EvaluateExpressionAsync("document.querySelector('#searchInput').value = ''");
                    await pupPage.TypeAsync("#searchInput", partNum);
                    await Task.Delay(1000);
                    if (await pupPage.EvaluateFunctionAsync<bool>("(aOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0] == null"))
                    {
                        dataItemProduct.IsCollectedFull = true;
                        dataItemProduct.IsActive = false;
                        dataItemProduct.IsAllBackOrdered = true;
                        dataItemProduct.Model = partNum;
                        staplesProdsFunc.Set(dataItemProduct);
                        dataItemProduct = staplesProdsFunc.getNextIsCollectedFullFalse();
                        continue;
                    }
                    url = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0].href");
                    if (!dataItemProduct.Url.Trim().ToLower().Equals(url.ToLower()))
                    {
                        // if(staplesProdsFunc.)
                        dataItemProduct.IsCollectedFull = true;
                        staplesProdsFunc.Set(dataItemProduct);

                        dataItemProduct = staplesProdsFunc.getNextIsCollectedFullFalse();
                        continue;
                        //dataItemProduct.Title = "WRONG URL IN STAPLES.COM MANNUAL CHECK NEEDED!" + url;
                        //dataItemProduct.IsCollectedFull = true;
                        //dataItemProduct.Model = partNum;
                        //staplesProdsFunc.Set(dataItemProduct);
                        //dataItemProduct = new();
                        //dataItemProduct.Model = partNum;
                        //dataItemProduct.Url = partNum;
                        //staplesProdsFunc.Add(dataItemProduct);
                    }
                    dataItemProduct.ImgMain = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_image')[0].src");
                    price = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__final_price')[0].textContent");
                    if (!string.IsNullOrEmpty(price))
                        if (double.TryParse(price.Replace("$", "").Trim().ToCharArray(), out double priceNum))
                            dataItemProduct.PriceBuyDef = priceNum;
                    dataItemProduct.Title = await pupPage.EvaluateFunctionAsync<string>("(imgOut)=>document.querySelector('#flyoutContent').getElementsByClassName('mini-tile__product_title_link')[0].innerText");
                    dataItemProduct.IsActive = true;
                    dataItemProduct.IsAllBackOrdered = false;
                    dataItemProduct.DateLastAvail = DateTime.Now.ToString();
                    dataItemProduct.PriceBuyCurrent = dataItemProduct.PriceBuyDef;
                    dataItemProduct.IsCollectedFull = true;
                    staplesProdsFunc.Set(dataItemProduct);
                    dataItemProduct = staplesProdsFunc.getNextIsCollectedFullFalse();
                }
            }
            catch (Exception e) { System.Console.Out.WriteLine(e.Message); return false; }
            return true;
        }

        private static async Task<CrawledPage> GetHtml(string _uri)
        {
            CrawlConfiguration crawlConfiguration = new()
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

            if (staplesPupPageRequester == null)
                staplesPupPageRequester = new(crawlConfiguration, new WebContentExtractor(), null);
            return await staplesPupPageRequester.MakeRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }
    }
}
