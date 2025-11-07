using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Crawler;
using Devweb.Poco;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System.Net;

namespace WebApp.Code.Crawler.Staples
{
    public class StaplesAppCrawler()
    {
        PageRequester? pageRequester;

        public async Task<bool> PrcsStplsCats()
        {
            ProdsFunc prodsFunc = new();
            ProdsFunc prodsFuncTemp = new();
            DataItemProduct dataItemProduct;
            DataItemProduct dataItemProductTemp;
            dataItemProduct = prodsFuncTemp.getNextIsCollectedFullFalse();
            while (dataItemProduct != null)
            {
                CrawledPage crawledPage = await PageRequesterCrawler(dataItemProduct.Url);
                //if (!ProcessProd(crawledPage, dataItemProduct))
                //{
                //    System.Console.Out.Write("Read error(Prod):" + dataItemProduct.Id + " ");
                //    return false;
                //}
                dataItemProductTemp = prodsFuncTemp.getNextIsCollectedFullFalse();
                if (dataItemProduct.Id == dataItemProductTemp.Id)
                    return false;
                dataItemProduct = dataItemProductTemp;
            }
            return true;
        }

        public async Task<bool> ProcessStaplesProd()
        {
            ProdsFunc prodsFunc = new();
            DataItemProduct dataItemProduct;
            DataItemProduct dataItemProductTemp;
            dataItemProduct = prodsFunc.getNextIsCollectedFullFalse();
            while (dataItemProduct != null)
            {
                CrawledPage crawledPage = await PageRequesterCrawler(dataItemProduct.Url);
                if (!ProcessProd(crawledPage, dataItemProduct))
                {
                    System.Console.Out.Write("Read error(Prod):" + dataItemProduct.Id + " ");
                    return false;
                }
                dataItemProductTemp = prodsFunc.getNextIsCollectedFullFalse();
                if (dataItemProduct.Id == dataItemProductTemp.Id)
                    return false;
                dataItemProduct = dataItemProductTemp;
            }
            return true;
        }

        public async Task<bool> FindGoogleCache()
        {
            ProdsFunc prodsFunc = new();

            DataItemProduct dataItemProduct;
            string result;
            CrawledPage crawledPage;
            dataItemProduct = prodsFunc.getNextIsCollectedFullFalse();
            while (dataItemProduct != null)
            {
                crawledPage = await PageRequesterCrawler(@"https://www.google.com/search?q=" + dataItemProduct.Url);
                if (crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.Contains(("u003dcache:")))
                {
                    result = crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.Substring(crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.IndexOf("u003dcache:") + 11, 12);
                    string googUri = @"https://webcache.googleusercontent.com/search?q=cache:" + result + ":" + dataItemProduct.Url;
                    crawledPage = await PageRequesterCrawler(googUri);

                    if (!ProcessProd(crawledPage, dataItemProduct))
                    {
                        System.Console.Out.Write("Read error(Prod):" + dataItemProduct.Id + " ");
                        return false;
                    }
                }
                else
                    System.Console.Out.WriteLine(dataItemProduct.Url);
                System.Console.Out.Write(dataItemProduct.Id + " ");
                //staplesProdsFunc.SetIsCollectedFullTrue(staplesProducts.Id);
                dataItemProduct = prodsFunc.getNextIsCollectedFullFalse();

                //if (!ProcessProd(crawledPage, staplesProducts))
                //    return false;

                //if (staplesProducts.Id == staplesProductsTemp.Id)
                //    return false;
                //staplesProducts = staplesProductsTemp;
            }
            return true;
        }

        private bool ProcessProd(CrawledPage crawledPage, DataItemProduct dataItemProduct)
        {
            ProdsFunc prodsFunc = new();
            if (crawledPage == null)
                return false;
            if (crawledPage.HttpResponseMessage == null)
                return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                dataItemProduct.Title = crawledPage.HttpResponseMessage.StatusCode.ToString();
                dataItemProduct.IsCollectedFull = true;
                prodsFunc.Set(dataItemProduct);
                return true;
            }
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode)
                return false;

            if (crawledPage.AngleSharpHtmlDocument == null)
                return false;
            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            DataItemProductList dataItemProductList = new();
            dataItemProductList.dataItemProduct = dataItemProduct;

            //Catg
            IHtmlCollection<IElement>? catgContainer;
            if (angleSharpHtmlDocument.GetElementById("breadcrumbs_container") != null && angleSharpHtmlDocument.GetElementById("breadcrumbs_container").FirstChild != null)
            {
                catgContainer = angleSharpHtmlDocument.GetElementById("breadcrumbs_container").QuerySelectorAll("li") ?? angleSharpHtmlDocument.GetElementById("breadcrumbs_container").QuerySelectorAll("li");
                dataItemProductList.dataItemCatgPerProd = new();
                dataItemProductList.dataItemCatgPerProd.Name1 = catgContainer.GetElementById("breadcrumb_1") == null ? null : (catgContainer.GetElementById("breadcrumb_1").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_1").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name2 = catgContainer.GetElementById("breadcrumb_2") == null ? null : (catgContainer.GetElementById("breadcrumb_2").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_2").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name3 = catgContainer.GetElementById("breadcrumb_3") == null ? null : (catgContainer.GetElementById("breadcrumb_3").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_3").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name4 = catgContainer.GetElementById("breadcrumb_4") == null ? null : (catgContainer.GetElementById("breadcrumb_4").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_4").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name5 = catgContainer.GetElementById("breadcrumb_5") == null ? null : (catgContainer.GetElementById("breadcrumb_5").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_5").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name6 = catgContainer.GetElementById("breadcrumb_6") == null ? null : (catgContainer.GetElementById("breadcrumb_6").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_6").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Name7 = catgContainer.GetElementById("breadcrumb_7") == null ? null : (catgContainer.GetElementById("breadcrumb_7").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_7").QuerySelector("a").TextContent);
                dataItemProductList.dataItemCatgPerProd.Href1 = catgContainer.GetElementById("breadcrumb_1") == null ? null : (catgContainer.GetElementById("breadcrumb_1").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_1").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_1").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href2 = catgContainer.GetElementById("breadcrumb_2") == null ? null : (catgContainer.GetElementById("breadcrumb_2").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_2").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_2").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href3 = catgContainer.GetElementById("breadcrumb_3") == null ? null : (catgContainer.GetElementById("breadcrumb_3").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_3").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_3").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href4 = catgContainer.GetElementById("breadcrumb_4") == null ? null : (catgContainer.GetElementById("breadcrumb_4").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_4").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_4").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href5 = catgContainer.GetElementById("breadcrumb_5") == null ? null : (catgContainer.GetElementById("breadcrumb_5").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_5").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_5").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href6 = catgContainer.GetElementById("breadcrumb_6") == null ? null : (catgContainer.GetElementById("breadcrumb_6").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_6").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_6").QuerySelector("a")).Href);
                dataItemProductList.dataItemCatgPerProd.Href7 = catgContainer.GetElementById("breadcrumb_7") == null ? null : (catgContainer.GetElementById("breadcrumb_7").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_7").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_7").QuerySelector("a")).Href);
            }
            //End Catg

            //Prices
            IElement? priceContainer;
            string? priceFinal = null, priceOriginal = null;
            double checkParse = 3.14;
            if (angleSharpHtmlDocument.GetElementById("priceInfoContainer") != null)
            {
                priceContainer = angleSharpHtmlDocument.GetElementById("priceInfoContainer");
                if (priceContainer.GetElementsByClassName("price-info__final_price_sku").Any())
                {
                    priceFinal = string.IsNullOrEmpty(priceContainer.GetElementsByClassName("price-info__final_price_sku").FirstOrDefault().TextContent) ? "" : priceContainer.GetElementsByClassName("price-info__final_price_sku").FirstOrDefault().TextContent;
                    if (!string.IsNullOrEmpty(priceFinal))
                        if (double.TryParse(Array.FindAll<char>(priceFinal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                            dataItemProductList.dataItemProduct.PriceBuyCurrent = double.Parse(Array.FindAll<char>(priceFinal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                }
                priceFinal = null;
                if (priceContainer.QuerySelectorAll("span").Any() && priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__originalPrice")).Any())
                {
                    priceOriginal = priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__originalPrice")).FirstOrDefault().TextContent;
                    if (!string.IsNullOrEmpty(priceOriginal))
                        if (double.TryParse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                            dataItemProductList.dataItemProduct.PriceBuyDef = double.Parse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                }
                else
                {
                    if (priceContainer.GetElementsByClassName("price-info__regular_price").Any())
                    {
                        priceOriginal = priceContainer.GetElementsByClassName("price-info__regular_price").FirstOrDefault().TextContent;
                        if (!string.IsNullOrEmpty(priceOriginal))
                            if (double.TryParse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                                dataItemProductList.dataItemProduct.PriceBuyDef = double.Parse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                    }
                }
                priceOriginal = null;
                if (dataItemProductList.dataItemProduct.PriceBuyCurrent != null && dataItemProductList.dataItemProduct.PriceBuyCurrent != 0)
                    if (dataItemProductList.dataItemProduct.PriceBuyDef == null || dataItemProductList.dataItemProduct.PriceBuyDef == 0)
                        dataItemProductList.dataItemProduct.PriceBuyDef = dataItemProductList.dataItemProduct.PriceBuyCurrent;

                dataItemProductList.dataItemProduct.UnitOfMeas = priceContainer.QuerySelectorAll("span").Any() ? (priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__uom")).Any() ? priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__uom")).FirstOrDefault().TextContent : "") : "";

            }
            DataItemPrices dataItemPrices = new()
            {
                Id = dataItemProductList.dataItemProduct.Id,
                PriceBuyCurrent = dataItemProductList.dataItemProduct.PriceBuyCurrent,
                PriceBuyDef = dataItemProductList.dataItemProduct.PriceBuyDef,
                IsOutOfStock = true,
                CityTo = angleSharpHtmlDocument.GetElementById("delivery_title") == null ? "" : string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("delivery_title").Text()) ? "" : angleSharpHtmlDocument.GetElementById("delivery_title").Text()
            };

            IHtmlDivElement? deliveryContainer = (IHtmlDivElement?)(angleSharpHtmlDocument.GetElementById("ONE_TIME_PURCHASE") == null ? null : angleSharpHtmlDocument.GetElementById("ONE_TIME_PURCHASE"));
            bool backOrderFlag = false;
            if (deliveryContainer != null)
            {
                if (deliveryContainer.QuerySelectorAll("span").Any())
                {
                    if (deliveryContainer.QuerySelectorAll("span").Where(x => x.Id != null && x.Id.Contains("delivery_title")).Any())
                        dataItemPrices.DeliveredBy = deliveryContainer.QuerySelectorAll("span").Where(x => x.Id != null && x.Id.Contains("delivery_title")).FirstOrDefault().TextContent;
                    if (deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("delivery-info-") && x.ClassName.Contains("__location")).Any())
                        dataItemPrices.StateTo = deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("delivery-info-") && x.ClassName.Contains("__location")).FirstOrDefault().TextContent;
                }
                List<IElement> deliveryContList = null;
                if (deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).Any())
                {
                    deliveryContList = deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).ToList<IElement>();
                    foreach (var x in deliveryContList)
                        if (x.TextContent.Contains("item is out of stock"))
                        {
                            dataItemPrices.IsBackOrdered = true;
                            dataItemPrices.IsOutOfStock = true;
                            backOrderFlag = true;
                        }
                }
                else
                {
                    if (deliveryContainer.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).Any())
                    {
                        deliveryContList = deliveryContainer.QuerySelectorAll("div").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).ToList<IElement>();
                        foreach (var x in deliveryContList)
                            if (x.TextContent.Contains("item is out of stock"))
                            {
                                dataItemPrices.IsBackOrdered = true;
                                dataItemPrices.IsOutOfStock = true;
                                backOrderFlag = true;
                            }
                    }
                }
            }
            if (backOrderFlag != true)
            {
                dataItemPrices.IsBackOrdered = false;
                dataItemPrices.DateLastInStock = DateTime.Now.ToString();
                dataItemPrices.IsOutOfStock = false;
            }
            dataItemProductList.dataItemPricesList.Add(dataItemPrices);
            //End of Prices

            //Product
            dataItemProductList.dataItemProduct.Title = angleSharpHtmlDocument.GetElementById("product_title") == null ? "" : angleSharpHtmlDocument.GetElementById("product_title").TextContent;
            dataItemProductList.dataItemProduct.Item = angleSharpHtmlDocument.GetElementById("item_number") == null ? "" : angleSharpHtmlDocument.GetElementById("item_number").TextContent.Replace("Item #: ", "");
            dataItemProductList.dataItemProduct.Model = angleSharpHtmlDocument.GetElementById("manufacturer_number") == null ? "" : angleSharpHtmlDocument.GetElementById("manufacturer_number").TextContent.Replace("Model #:", "").Trim();
            dataItemProductList.dataItemProduct.IsActive = false;
            dataItemProductList.dataItemProduct.IsAllBackOrdered = null;
            foreach (DataItemPrices p in dataItemProductList.dataItemPricesList)
            {
                if (p.IsOutOfStock == false)
                {
                    dataItemProductList.dataItemProduct.DateLastAvail = dataItemPrices.DateLastInStock;
                    dataItemProductList.dataItemProduct.IsAllBackOrdered = false;
                    dataItemProductList.dataItemProduct.IsActive = true;
                }
            }

            //Product option and varieties
            dataItemProductList.dataItemProduct.ProductOptionsHtml = angleSharpHtmlDocument.GetElementsByClassName("sku-set__product_skuset").Any() ? angleSharpHtmlDocument.GetElementsByClassName("sku-set__product_skuset").FirstOrDefault().OuterHtml : null;
            if (dataItemProductList.dataItemProduct.ProductOptionsHtml != null)
                dataItemProductList.dataItemProduct.ProductOptionsHtml = WebUtility.HtmlEncode(dataItemProductList.dataItemProduct.ProductOptionsHtml);

            //Image Gallery
            if (angleSharpHtmlDocument.GetElementById("image_gallery_container") != null)
                dataItemProductList.dataItemProduct.ImgsGalleryHtml = string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("image_gallery_container").OuterHtml) ? "" : WebUtility.HtmlEncode(angleSharpHtmlDocument.GetElementById("image_gallery_container").OuterHtml);

            //Imgs
            IHtmlCollection<IElement>? imagesContainer = angleSharpHtmlDocument.GetElementsByClassName("carousel__slider_container").Any() ? angleSharpHtmlDocument.GetElementsByClassName("carousel__slider_container") : null;
            DataItemPics dataItemPics;
            int orderNum = 1;
            if (imagesContainer != null)
                foreach (IHtmlDivElement _e in imagesContainer)
                {
                    dataItemPics = new();
                    dataItemPics.OrderNum = orderNum++;
                    if (_e.QuerySelector("img") == null)
                        continue;
                    dataItemPics.Url = _e.QuerySelector("img").GetAttribute("srcset").Trim();
                    dataItemPics.Url = dataItemPics.Url.Substring(0, dataItemPics.Url.IndexOf(' '));
                    if (string.IsNullOrEmpty(dataItemProductList.dataItemProduct.ImgMain))
                        dataItemProductList.dataItemProduct.ImgMain = dataItemPics.Url;
                    dataItemProductList.dataItemPicsList.Add(dataItemPics);
                }

            //Highlights
            if (angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper") != null)
                if (angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul") != null)
                    if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul").OuterHtml))
                        dataItemProductList.dataItemProduct.HighlightsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul").OuterHtml);

            //Details
            if (angleSharpHtmlDocument.GetElementById("ProdDescSpec") != null)
                if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProdDescSpec").OuterHtml))
                    dataItemProductList.dataItemProduct.DetailsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProdDescSpec").OuterHtml);

            //Specs
            IHtmlTableElement? specsContainer;
            DataItemSpecs dataItemSpecs;
            IHtmlTableSectionElement specsTableBody;
            if (angleSharpHtmlDocument.GetElementById("ProdSpecSection") != null)
            {
                if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProdSpecSection").OuterHtml))
                    dataItemProductList.dataItemProduct.SpecificationsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProdSpecSection").OuterHtml);
                if (angleSharpHtmlDocument.GetElementById("ProdSpecSection").FirstChild != null)
                {
                    specsContainer = (IHtmlTableElement?)angleSharpHtmlDocument.GetElementById("ProdSpecSection").FirstChild;
                    if (specsContainer.Bodies.Any())
                    {
                        specsTableBody = specsContainer.Bodies.FirstOrDefault();
                        orderNum = 1;
                        foreach (IHtmlTableRowElement _e in specsTableBody.Rows)
                        {
                            dataItemSpecs = new();
                            dataItemSpecs.Name = _e.Cells[0] == null ? "" : string.IsNullOrEmpty(_e.Cells[0].TextContent) ? "" : _e.Cells[0].TextContent;
                            dataItemSpecs.Info = _e.Cells[1] == null ? "" : string.IsNullOrEmpty(_e.Cells[1].TextContent) ? "" : _e.Cells[1].TextContent;
                            dataItemSpecs.OrderNum = orderNum++;
                            if (!string.IsNullOrEmpty(_e.OuterHtml))
                                dataItemSpecs.Html = WebUtility.UrlEncode(_e.OuterHtml);
                            dataItemProductList.dataItemSpecsList.Add(dataItemSpecs);
                        }
                    }
                }
            }
            //End Specs

            dataItemProductList.dataItemProduct.IsCollectedFull = true;
            prodsFunc.Add(dataItemProductList);
            return true;
        }

        private static bool ProcessStaplesCatgs(CrawledPage crawledPage)
        {
            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            //var htmlLinkElements = angleSharpHtmlDocument.GetElementsByTagName("a").Where(x => x != null && !string.IsNullOrWhiteSpace(((IHtmlAnchorElement)x).Href) && ((IHtmlAnchorElement)x).Href.Contains("www.staplesadvantage.com/"));
            var htmlLinkElements = angleSharpHtmlDocument.GetElementsByTagName("a").Where(x => x != null && !string.IsNullOrWhiteSpace(((IHtmlAnchorElement)x).Href));
            List<string> linksList = new();

            foreach (var _i in htmlLinkElements)
            {
                if (((IHtmlAnchorElement)_i).Href.Contains("https://www.staplesadvantage.com/SuperCategory") || ((IHtmlAnchorElement)_i).Href.Contains("https://www.staplesadvantage.com/CategoryBrowse") || ((IHtmlAnchorElement)_i).Href.Contains("https://www.staplesadvantage.com/browse"))
                {
                    linksList.Add(((IHtmlAnchorElement)_i).Href);
                }
                else
                if (((IHtmlAnchorElement)_i).Href.Contains("/SuperCategory?") || ((IHtmlAnchorElement)_i).Href.Contains("/CategoryBrowse?") || ((IHtmlAnchorElement)_i).Href.Contains("/browse"))
                {
                    linksList.Add("https://www.staplesadvantage.com" + ((IHtmlAnchorElement)_i).Href.Replace("about://", ""));
                }
            }
            linksList.Sort();
            string superCatgTmp = " ";
            string catgBrowseCatgTmp = " ";
            string browseTmp = " ";
            string str1;
            CatgsFunc catgsFunc = new();

            foreach (string _s in linksList)
            {
                if (_s.Contains("https://www.staplesadvantage.com/SuperCategory") && _s.ToLower().Contains("name="))
                {
                    if (_s.Contains('&') && _s.IndexOf('&') > _s.Length)
                    {
                        str1 = _s.Substring(0, _s.IndexOf('&'));
                        if (!str1.ToLower().Contains("name="))
                            str1 = _s;
                    }
                    else
                        str1 = _s;
                    if (!str1.Equals(superCatgTmp))
                    {
                        superCatgTmp = str1;
                        catgsFunc.AddUrl(str1);
                    }
                }
                else
                if (_s.Contains("https://www.staplesadvantage.com/CategoryBrowse"))
                {
                    if (_s.ToLower().Contains("id=") && _s.ToLower().Contains("type=") && _s.ToLower().Contains("&"))
                    {
                        str1 = "https://www.staplesadvantage.com/CategoryBrowse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3);
                        if (str1.Substring(str1.IndexOf("&")).IndexOf("&") > 0)
                            str1 = "https://www.staplesadvantage.com/CategoryBrowse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3, _s.Length - str1.Substring(str1.IndexOf("&")).IndexOf("&"));
                    }
                    else
                        continue;

                    if (!str1.Equals(catgBrowseCatgTmp))
                    {
                        catgBrowseCatgTmp = _s;
                        catgsFunc.AddUrl(str1);
                    }
                }
                else
                if (_s.Contains("https://www.staplesadvantage.com/browse"))
                {
                    if (_s.ToLower().Contains("id=") && _s.ToLower().Contains("type=") && _s.ToLower().Contains("&"))
                    {
                        str1 = "https://www.staplesadvantage.com/browse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3);
                        if (str1.Substring(str1.IndexOf("&")).IndexOf("&") > 0)
                            str1 = "https://www.staplesadvantage.com/browse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3, _s.Length - str1.Substring(str1.IndexOf("&")).IndexOf("&"));
                    }
                    else
                        continue;

                    if (!str1.Equals(browseTmp))
                    {
                        browseTmp = _s;
                        catgsFunc.AddUrl(str1);
                    }
                }
            }
            return true;
        }

        private async Task<CrawledPage> PupPageRequesterCrawler(string _uri)
        {
            CrawlConfiguration crawlConfiguration = new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
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

            pageRequester ??= new(crawlConfiguration, new WebContentExtractor(), null);
            return await pageRequester.MakeRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }

        private async Task<CrawledPage> PageRequesterCrawler(string _uri)
        {
            CrawlConfiguration crawlConfiguration = new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
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

            pageRequester ??= new(crawlConfiguration, new WebContentExtractor(), null);
            return await pageRequester.MakeRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }

        private static PoliteWebCrawler SetupCrawler()
        {
            PoliteWebCrawler politeWebCrawler = new(
                null,  //GetCrawlConfig(),
                new CrawlDecisionMaker(),
                null,  //new ThreadMgr(),
                null,  //new Scheduler(),
                null,  //new PageRequester(),
                null,  //new HyperLinkParser(),
                null,  //new MemoryManager(),
                null,  //new DomainRateLimiter,
                null   //new RobotsDotTextFinder()
            );
            return politeWebCrawler;
        }

        private void politeWebCrawler_PageCrawlStarting(object sender, PageCrawlStartingArgs e) { }

        private void politeWebCrawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e) { }
    }
}