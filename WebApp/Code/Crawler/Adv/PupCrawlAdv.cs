using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Poco;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System.Net;

namespace WebApp.Code.Crawler.Adv
{
    public class PupCrawlAdv
    {
        static PupPageRequester? pupPageRequester;

        public async Task<bool> PrcsAdvProdTables()
        {
            DataItemCatg? dataItemCatg, dataItemCatgTemp = null;
            CatgsFunc? catgsFunc = new();
            CrawledPage? crawledPage;
            int? errorReload = 0;

            dataItemCatg = catgsFunc.GetNextIsCollectedHRefFalse();
            while (dataItemCatg != null)
            {
                if (dataItemCatgTemp != null && dataItemCatgTemp.Id == dataItemCatg.Id)
                {
                    if (errorReload == 0)
                        errorReload++;
                    else
                    {
                        System.Console.Out.WriteLine("Same CatId received by getNextIsCollectedHRefFalse:" + dataItemCatg.Url);
                        return false;
                    }
                }
                else
                    errorReload = 0;
                dataItemCatgTemp = dataItemCatg;

                crawledPage = await GetHtml(dataItemCatg.Url);
                if (!PrcsRowsPerPage(crawledPage)) { System.Console.Out.WriteLine("Error in PrcsRowsPerPage(crawledPage) :" + dataItemCatg.Url); return false; }
                catgsFunc.SetIsCollectedHRefToTrue(dataItemCatg.Id);
                dataItemCatg = catgsFunc.GetNextIsCollectedHRefFalse();
            }
            return true;
        }

        private bool PrcsRowsPerPage(CrawledPage crawledPage)
        {
            if (crawledPage == null) return false;
            if (crawledPage.HttpResponseMessage == null) return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return false;
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode) return false;
            if (crawledPage.AngleSharpHtmlDocument == null) return false;

            IHtmlDocument? angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CatgsFunc? catgsFunc = new();
            ProdsFunc? prodsFunc = new();
            DataItemProduct? dataItemProduct = null;
            int? count = 0;

            IHtmlDivElement? divDowTable = angleSharpHtmlDocument.GetElementsByClassName("grid__row").Any() ? (IHtmlDivElement)angleSharpHtmlDocument.GetElementsByClassName("grid__row")[1] : null;
            if (divDowTable == null) { System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " had no products."); return true; }
            foreach (IHtmlDivElement htmlDivElement in divDowTable.Children)
            {
                double priceNum = 0;
                string? price = null, _link = null, shortProdUrl = null;
                IHtmlDivElement? divTemp = null;
                count++;
                var a = htmlDivElement.QuerySelectorAll("a");

                if (a == null) { System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " Product #" + count.ToString() + "bad html"); continue; }
                if (a[0] != null)
                    _link = ((IHtmlAnchorElement)a[0]).Href.Trim().Replace(@"about://", @"https://www.staples.com");
                else
                if (a[1] != null)
                    _link = ((IHtmlAnchorElement)a[1]).Href.Trim().Replace(@"about://", @"https://www.staples.com");

                if (_link == null) { System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " Product #" + count.ToString() + "bad html"); continue; }

                shortProdUrl = _link.Substring(_link.ToLower().IndexOf("/product") + 1);
                if (shortProdUrl == null) { System.Console.Out.WriteLine(crawledPage.Uri.AbsoluteUri + " Product #" + count.ToString() + "bad html"); continue; }

                if (prodsFunc.isExistByShortProdUrl(shortProdUrl))
                    dataItemProduct = prodsFunc.getByShortProdUrl(shortProdUrl);
                if (dataItemProduct == null)
                {
                    dataItemProduct = new();
                    dataItemProduct.Url = _link;
                    prodsFunc.Add(dataItemProduct);
                }
                IHtmlImageElement? img = htmlDivElement.QuerySelectorAll("img").Any() ? ((IHtmlImageElement?)htmlDivElement.QuerySelectorAll("img").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__image")).FirstOrDefault()) : null;
                dataItemProduct.ImgMain = img == null ? null : img.Source;
                divTemp = (IHtmlDivElement?)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__title")).FirstOrDefault();
                if (divTemp != null) dataItemProduct.Title = divTemp.QuerySelector("a").TextContent;
                price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__final_price false")).FirstOrDefault().TextContent;
                if (!string.IsNullOrEmpty(price))
                    if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out priceNum))
                        dataItemProduct.PriceBuyCurrentAdv = priceNum;
                if (htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__original_price")).Any())
                {
                    price = htmlDivElement.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__original_price")).FirstOrDefault().TextContent;
                    if (!string.IsNullOrEmpty(price))
                        if (double.TryParse(price.Replace("Final price", "").Replace("$", "").Trim().ToCharArray(), out priceNum))
                            dataItemProduct.PriceBuyDefAdv = priceNum;
                }
                if (dataItemProduct.PriceBuyDefAdv == 0) dataItemProduct.PriceBuyDefAdv = dataItemProduct.PriceBuyCurrentAdv;
                divTemp = (IHtmlDivElement?)htmlDivElement.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("standard-tile__price_per_unit")).FirstOrDefault();
                if (divTemp != null) { if (divTemp.FirstChild != null) dataItemProduct.UnitOfMeas = divTemp.FirstChild.TextContent; }

                dataItemProduct.Model = dataItemProduct.Url.Substring(dataItemProduct.Url.IndexOf("product_") + 8).Trim();
                dataItemProduct.IsAllBackOrdered = false;
                dataItemProduct.DateLastAvail = DateTime.Now.ToString();
                dataItemProduct.IsCollectedFull = true;
                dataItemProduct.IsActive = true;
                dataItemProduct.ShortProdUrl = shortProdUrl;
                prodsFunc.Set(dataItemProduct);
                dataItemProduct = null;
            }
            return true;
        }

        private static async Task<CrawledPage> GetHtml(string _uri)
        {
            CrawlConfiguration? crawlConfiguration = new CrawlConfiguration
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
