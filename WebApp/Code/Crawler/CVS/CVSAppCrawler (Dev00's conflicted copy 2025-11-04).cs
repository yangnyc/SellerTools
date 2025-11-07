using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Devweb.Core;
using Devweb.Crawler;
using Devweb.Poco;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApp.Code.Crawler.CVS
{
    public class CVSAppCrawler()
    {
        //StaplesPageRequester? pageRequester;

        /*public async Task<bool> PrcsCVSCats()
        {
            CVSProdsFunc CVSProdsFunc = new CVSProdsFunc();
            CVSProdsFunc TempCVSProdsFunc = new CVSProdsFunc();
            CVSProducts CVSProducts;
            CVSProducts CVSProductsTemp;
            CVSProducts = TempCVSProdsFunc.getNextIsCollectedFullFalse();
            while (CVSProducts != null)
            {
                CrawledPage crawledPage = await PageRequesterCrawler(CVSProducts.Url);
                //if (!ProcessProd(crawledPage, CVSProducts))
                //{
                //    System.Console.Out.Write("Read error(Prod):" + CVSProducts.Id + " ");
                //    return false;
                //}
                CVSProductsTemp = TempCVSProdsFunc.getNextIsCollectedFullFalse();
                if (CVSProducts.Id == CVSProductsTemp.Id)
                    return false;
                CVSProducts = CVSProductsTemp;
            }
            return true;
        }*/

        /*public async Task<bool> ProcessCVSProd()
        {
            CVSProdsFunc CVSProdsFunc = new CVSProdsFunc();
            CVSProdsFunc TempCVSProdsFunc = new CVSProdsFunc();
            CVSProducts CVSProducts;
            CVSProducts CVSProductsTemp;
            CVSProducts = TempCVSProdsFunc.getNextIsCollectedFullFalse();
            while (CVSProducts != null)
            {
                CrawledPage crawledPage = await PageRequesterCrawler(CVSProducts.Url);
                if (!ProcessProd(crawledPage, CVSProducts))
                {
                    System.Console.Out.Write("Read error(Prod):" + CVSProducts.Id + " ");
                    return false;
                }
                CVSProductsTemp = TempCVSProdsFunc.getNextIsCollectedFullFalse();
                if (CVSProducts.Id == CVSProductsTemp.Id)
                    return false;
                CVSProducts = CVSProductsTemp;
            }
            return true;
        }
*/
        /* public async Task<bool> FindGoogleCache()
         {
             CVSProdsFunc TempCVSProdsFunc = new CVSProdsFunc();

             CVSProducts CVSProducts;
             string result;
             CrawledPage crawledPage;
             CVSProducts = TempCVSProdsFunc.getNextIsCollectedFullFalse();
             while (CVSProducts != null)
             {
                 crawledPage = await PageRequesterCrawler(@"https://www.google.com/search?q=" + CVSProducts.Url);
                 if (crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.Contains(("u003dcache:")))
                 {
                     result = crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.Substring(crawledPage.AngleSharpHtmlDocument.Body.InnerHtml.IndexOf("u003dcache:") + 11, 12);
                     string googUri = @"https://webcache.googleusercontent.com/search?q=cache:" + result + ":" + CVSProducts.Url;
                     crawledPage = await PageRequesterCrawler(googUri);

                     if (!ProcessProd(crawledPage, CVSProducts))
                     {
                         System.Console.Out.Write("Read error(Prod):" + CVSProducts.Id + " ");
                         return false;
                     }
                 }
                 else
                     System.Console.Out.WriteLine(CVSProducts.Url);
                 System.Console.Out.Write(CVSProducts.Id + " ");
                 //CVSProdsFunc.SetIsCollectedFullTrue(CVSProducts.Id);
                 CVSProducts = TempCVSProdsFunc.getNextIsCollectedFullFalse();

                 //if (!ProcessProd(crawledPage, CVSProducts))
                 //    return false;

                 //if (CVSProducts.Id == CVSProductsTemp.Id)
                 //    return false;
                 //CVSProducts = CVSProductsTemp;
             }
             return true;
         }
 */
        /*private bool ProcessProd(CrawledPage crawledPage, CVSProducts CVSProducts)
        {
            CVSProdsFunc CVSProdsFunc = new CVSProdsFunc();
            if (crawledPage == null)
                return false;
            if (crawledPage.HttpResponseMessage == null)
                return false;
            if (crawledPage.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                CVSProducts.Title = crawledPage.HttpResponseMessage.StatusCode.ToString();
                CVSProducts.IsCollectedFull = true;
                CVSProdsFunc.Set(CVSProducts);
                return true;
            }
            if (!crawledPage.HttpResponseMessage.IsSuccessStatusCode)
                return false;

            if (crawledPage.AngleSharpHtmlDocument == null)
                return false;
            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            CVSProd CVSProd = new();
            CVSProd.CVSProducts = CVSProducts;

            //Catg
            IHtmlCollection<IElement> catgContainer;
            if (angleSharpHtmlDocument.GetElementById("breadcrumbs_container") != null && angleSharpHtmlDocument.GetElementById("breadcrumbs_container").FirstChild != null)
            {
                catgContainer = angleSharpHtmlDocument.GetElementById("breadcrumbs_container").QuerySelectorAll("li") ?? angleSharpHtmlDocument.GetElementById("breadcrumbs_container").QuerySelectorAll("li");
                CVSProd.CVSCatgPerProd = new CVSCatgPerProd();
                CVSProd.CVSCatgPerProd.Name1 = catgContainer.GetElementById("breadcrumb_1") == null ? null : (catgContainer.GetElementById("breadcrumb_1").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_1").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name2 = catgContainer.GetElementById("breadcrumb_2") == null ? null : (catgContainer.GetElementById("breadcrumb_2").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_2").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name3 = catgContainer.GetElementById("breadcrumb_3") == null ? null : (catgContainer.GetElementById("breadcrumb_3").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_3").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name4 = catgContainer.GetElementById("breadcrumb_4") == null ? null : (catgContainer.GetElementById("breadcrumb_4").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_4").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name5 = catgContainer.GetElementById("breadcrumb_5") == null ? null : (catgContainer.GetElementById("breadcrumb_5").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_5").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name6 = catgContainer.GetElementById("breadcrumb_6") == null ? null : (catgContainer.GetElementById("breadcrumb_6").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_6").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Name7 = catgContainer.GetElementById("breadcrumb_7") == null ? null : (catgContainer.GetElementById("breadcrumb_7").QuerySelector("a") == null ? null : catgContainer.GetElementById("breadcrumb_7").QuerySelector("a").TextContent);
                CVSProd.CVSCatgPerProd.Href1 = catgContainer.GetElementById("breadcrumb_1") == null ? null : (catgContainer.GetElementById("breadcrumb_1").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_1").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_1").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href2 = catgContainer.GetElementById("breadcrumb_2") == null ? null : (catgContainer.GetElementById("breadcrumb_2").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_2").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_2").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href3 = catgContainer.GetElementById("breadcrumb_3") == null ? null : (catgContainer.GetElementById("breadcrumb_3").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_3").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_3").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href4 = catgContainer.GetElementById("breadcrumb_4") == null ? null : (catgContainer.GetElementById("breadcrumb_4").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_4").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_4").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href5 = catgContainer.GetElementById("breadcrumb_5") == null ? null : (catgContainer.GetElementById("breadcrumb_5").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_5").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_5").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href6 = catgContainer.GetElementById("breadcrumb_6") == null ? null : (catgContainer.GetElementById("breadcrumb_6").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_6").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_6").QuerySelector("a")).Href);
                CVSProd.CVSCatgPerProd.Href7 = catgContainer.GetElementById("breadcrumb_7") == null ? null : (catgContainer.GetElementById("breadcrumb_7").QuerySelector("a") == null ? null : ((IHtmlAnchorElement)catgContainer.GetElementById("breadcrumb_7").QuerySelector("a")).Href ?? ((IHtmlLinkElement)catgContainer.GetElementById("breadcrumb_7").QuerySelector("a")).Href);
            }
            //End Catg

            //Prices
            IElement priceContainer;
            string priceFinal = null, priceOriginal = null;
            double checkParse = 3.14;
            if (angleSharpHtmlDocument.GetElementById("priceInfoContainer") != null)
            {
                priceContainer = angleSharpHtmlDocument.GetElementById("priceInfoContainer");
                if (priceContainer.GetElementsByClassName("price-info__final_price_sku").Any())
                {
                    priceFinal = string.IsNullOrEmpty(priceContainer.GetElementsByClassName("price-info__final_price_sku").FirstOrDefault().TextContent) ? "" : priceContainer.GetElementsByClassName("price-info__final_price_sku").FirstOrDefault().TextContent;
                    if (!string.IsNullOrEmpty(priceFinal))
                        if (double.TryParse(Array.FindAll<char>(priceFinal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                            CVSProd.CVSProducts.PriceBuyCurrentCVS = double.Parse(Array.FindAll<char>(priceFinal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                }
                priceFinal = null;
                if (priceContainer.QuerySelectorAll("span").Any() && priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__originalPrice")).Any())
                {
                    priceOriginal = priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__originalPrice")).FirstOrDefault().TextContent;
                    if (!string.IsNullOrEmpty(priceOriginal))
                        if (double.TryParse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                            CVSProd.CVSProducts.PriceBuyDefCVS = double.Parse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                }
                else
                {
                    if (priceContainer.GetElementsByClassName("price-info__regular_price").Any())
                    {
                        priceOriginal = priceContainer.GetElementsByClassName("price-info__regular_price").FirstOrDefault().TextContent;
                        if (!string.IsNullOrEmpty(priceOriginal))
                            if (double.TryParse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))), out checkParse))
                                CVSProd.CVSProducts.PriceBuyDefCVS = double.Parse(Array.FindAll<char>(priceOriginal.ToCharArray(), (c => (char.IsLetterOrDigit(c) || c == '.'))));
                    }
                }
                priceOriginal = null;
                if (CVSProd.CVSProducts.PriceBuyCurrentCVS != null && CVSProd.CVSProducts.PriceBuyCurrentCVS != 0)
                    if (CVSProd.CVSProducts.PriceBuyDefCVS == null || CVSProd.CVSProducts.PriceBuyDefCVS == 0)
                        CVSProd.CVSProducts.PriceBuyDefCVS = CVSProd.CVSProducts.PriceBuyCurrentCVS;

                CVSProd.CVSProducts.UnitOfMeas = priceContainer.QuerySelectorAll("span").Any() ? (priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__uom")).Any() ? priceContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("price-info__uom")).FirstOrDefault().TextContent : "") : "";

            }
            CVSPrices CVSPrices = new();
            CVSPrices.ProductId = CVSProd.CVSProducts.Id;
            CVSPrices.PriceBuyCurrent = CVSProd.CVSProducts.PriceBuyCurrentCVS;
            CVSPrices.PriceBuyDef = CVSProd.CVSProducts.PriceBuyDefCVS;
            CVSPrices.IsOutOfStock = true;
            CVSPrices.CityTo = angleSharpHtmlDocument.GetElementById("delivery_title") == null ? "" : string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("delivery_title").Text()) ? "" : angleSharpHtmlDocument.GetElementById("delivery_title").Text();

            IHtmlDivElement deliveryContainer = (IHtmlDivElement)(angleSharpHtmlDocument.GetElementById("ONE_TIME_PURCHASE") == null ? null : angleSharpHtmlDocument.GetElementById("ONE_TIME_PURCHASE"));
            bool backOrderFlag = false;
            if (deliveryContainer != null)
            {
                if (deliveryContainer.QuerySelectorAll("span").Any())
                {
                    if (deliveryContainer.QuerySelectorAll("span").Where(x => x.Id != null && x.Id.Contains("delivery_title")).Any())
                        CVSPrices.DeliveredBy = deliveryContainer.QuerySelectorAll("span").Where(x => x.Id != null && x.Id.Contains("delivery_title")).FirstOrDefault().TextContent;
                    if (deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("delivery-info-") && x.ClassName.Contains("__location")).Any())
                        CVSPrices.StateTo = deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("delivery-info-") && x.ClassName.Contains("__location")).FirstOrDefault().TextContent;
                }
                List<IElement> deliveryContList = null;
                if (deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).Any())
                {
                    deliveryContList = deliveryContainer.QuerySelectorAll("span").Where(x => x.ClassName != null && x.ClassName.Contains("notificationBubble__content")).ToList<IElement>();
                    foreach (var x in deliveryContList)
                        if (x.TextContent.Contains("item is out of stock"))
                        {
                            CVSPrices.IsBackOrdered = true;
                            CVSPrices.IsOutOfStock = true;
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
                                CVSPrices.IsBackOrdered = true;
                                CVSPrices.IsOutOfStock = true;
                                backOrderFlag = true;
                            }
                    }
                }
            }
            if (backOrderFlag != true)
            {
                CVSPrices.IsBackOrdered = false;
                CVSPrices.DateLastInStock = DateTime.Now.ToString();
                CVSPrices.IsOutOfStock = false;
            }
            CVSProd.CVSPricesList.Add(CVSPrices);
            //End of Prices

            //Product
            CVSProd.CVSProducts.Title = angleSharpHtmlDocument.GetElementById("product_title") == null ? "" : angleSharpHtmlDocument.GetElementById("product_title").TextContent;
            CVSProd.CVSProducts.Item = angleSharpHtmlDocument.GetElementById("item_number") == null ? "" : angleSharpHtmlDocument.GetElementById("item_number").TextContent.Replace("Item #: ", "");
            CVSProd.CVSProducts.Model = angleSharpHtmlDocument.GetElementById("manufacturer_number") == null ? "" : angleSharpHtmlDocument.GetElementById("manufacturer_number").TextContent.Replace("Model #:", "").Trim();
            CVSProd.CVSProducts.IsActive = false;
            CVSProd.CVSProducts.IsAllBackOrdered = null;
            foreach (CVSPrices p in CVSProd.CVSPricesList)
            {
                if (p.IsOutOfStock == false)
                {
                    CVSProd.CVSProducts.DateLastAvail = CVSPrices.DateLastInStock;
                    CVSProd.CVSProducts.IsAllBackOrdered = false;
                    CVSProd.CVSProducts.IsActive = true;
                }
            }

            //Product option and varieties
            CVSProd.CVSProducts.ProductOptionsHtml = angleSharpHtmlDocument.GetElementsByClassName("sku-set__product_skuset").Any() ? angleSharpHtmlDocument.GetElementsByClassName("sku-set__product_skuset").FirstOrDefault().OuterHtml : null;
            if (CVSProd.CVSProducts.ProductOptionsHtml != null)
                CVSProd.CVSProducts.ProductOptionsHtml = WebUtility.HtmlEncode(CVSProd.CVSProducts.ProductOptionsHtml);

            //Image Gallery
            if (angleSharpHtmlDocument.GetElementById("image_gallery_container") != null)
                CVSProd.CVSProducts.ImgsGalleryHtml = string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("image_gallery_container").OuterHtml) ? "" : WebUtility.HtmlEncode(angleSharpHtmlDocument.GetElementById("image_gallery_container").OuterHtml);

            //Imgs
            IHtmlCollection<IElement> imagesContainer = angleSharpHtmlDocument.GetElementsByClassName("carousel__slider_container").Any() ? angleSharpHtmlDocument.GetElementsByClassName("carousel__slider_container") : null;
            CVSPics CVSPics;
            int orderNum = 1;
            if (imagesContainer != null)
                foreach (IHtmlDivElement _e in imagesContainer)
                {
                    CVSPics = new();
                    CVSPics.OrderNum = orderNum++;
                    if (_e.QuerySelector("img") == null)
                        continue;
                    CVSPics.Url = _e.QuerySelector("img").GetAttribute("srcset").Trim();
                    CVSPics.Url = CVSPics.Url.Substring(0, CVSPics.Url.IndexOf(' '));
                    if (string.IsNullOrEmpty(CVSProd.CVSProducts.ImgMain))
                        CVSProd.CVSProducts.ImgMain = CVSPics.Url;
                    CVSProd.CVSPicsList.Add(CVSPics);
                }

            //Highlights
            if (angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper") != null)
                if (angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul") != null)
                    if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul").OuterHtml))
                        CVSProd.CVSProducts.HighlightsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProductDetailsSummaryWrapper").QuerySelector("ul").OuterHtml);

            //Details
            if (angleSharpHtmlDocument.GetElementById("ProdDescSpec") != null)
                if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProdDescSpec").OuterHtml))
                    CVSProd.CVSProducts.DetailsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProdDescSpec").OuterHtml);

            //Specs
            IHtmlTableElement specsContainer;
            CVSSpecs CVSSpecs;
            IHtmlTableSectionElement specsTableBody;
            if (angleSharpHtmlDocument.GetElementById("ProdSpecSection") != null)
            {
                if (!string.IsNullOrEmpty(angleSharpHtmlDocument.GetElementById("ProdSpecSection").OuterHtml))
                    CVSProd.CVSProducts.SpecificationsHtml = WebUtility.UrlEncode(angleSharpHtmlDocument.GetElementById("ProdSpecSection").OuterHtml);
                if (angleSharpHtmlDocument.GetElementById("ProdSpecSection").FirstChild != null)
                {
                    specsContainer = (IHtmlTableElement)angleSharpHtmlDocument.GetElementById("ProdSpecSection").FirstChild;
                    if (specsContainer.Bodies.Any())
                    {
                        specsTableBody = specsContainer.Bodies.FirstOrDefault();
                        orderNum = 1;
                        foreach (IHtmlTableRowElement _e in specsTableBody.Rows)
                        {
                            CVSSpecs = new();
                            CVSSpecs.Name = _e.Cells[0] == null ? "" : string.IsNullOrEmpty(_e.Cells[0].TextContent) ? "" : _e.Cells[0].TextContent;
                            CVSSpecs.Info = _e.Cells[1] == null ? "" : string.IsNullOrEmpty(_e.Cells[1].TextContent) ? "" : _e.Cells[1].TextContent;
                            CVSSpecs.OrderNum = orderNum++;
                            if (!string.IsNullOrEmpty(_e.OuterHtml))
                                CVSSpecs.Html = WebUtility.UrlEncode(_e.OuterHtml);
                            CVSProd.CVSSpecsList.Add(CVSSpecs);
                        }
                    }
                }
            }
            //End Specs

            CVSProd.CVSProducts.IsCollectedFull = true;
            CVSProdsFunc.Add(CVSProd);
            return true;
        }*/

        private static bool ProcessCVSCatgs(CrawledPage crawledPage)
        {
            IHtmlDocument angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument;
            //var htmlLinkElements = angleSharpHtmlDocument.GetElementsByTagName("a").Where(x => x != null && !string.IsNullOrWhiteSpace(((IHtmlAnchorElement)x).Href) && ((IHtmlAnchorElement)x).Href.Contains("www.CVSadvantage.com/"));
            var htmlLinkElements = angleSharpHtmlDocument.GetElementsByTagName("a").Where(x => x != null && !string.IsNullOrWhiteSpace(((IHtmlAnchorElement)x).Href));
            List<string> linksList = new();

            foreach (var _i in htmlLinkElements)
            {
                if (((IHtmlAnchorElement)_i).Href.Contains("https://www.CVSadvantage.com/SuperCategory") || ((IHtmlAnchorElement)_i).Href.Contains("https://www.CVSadvantage.com/CategoryBrowse") || ((IHtmlAnchorElement)_i).Href.Contains("https://www.CVSadvantage.com/browse"))
                {
                    linksList.Add(((IHtmlAnchorElement)_i).Href);
                }
                else
                if (((IHtmlAnchorElement)_i).Href.Contains("/SuperCategory?") || ((IHtmlAnchorElement)_i).Href.Contains("/CategoryBrowse?") || ((IHtmlAnchorElement)_i).Href.Contains("/browse"))
                {
                    linksList.Add("https://www.CVSadvantage.com" + ((IHtmlAnchorElement)_i).Href.Replace("about://", ""));
                }
            }
            linksList.Sort();
            string superCatgTmp = " ";
            string catgBrowseCatgTmp = " ";
            string browseTmp = " ";
            string str1;
            CatgsFunc CVSCatgsFunc = new();

            foreach (string _s in linksList)
            {
                if (_s.Contains("https://www.CVSadvantage.com/SuperCategory") && _s.ToLower().Contains("name="))
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
                        CVSCatgsFunc.AddUrl(str1);
                    }
                }
                else
                if (_s.Contains("https://www.CVSadvantage.com/CategoryBrowse"))
                {
                    if (_s.ToLower().Contains("id=") && _s.ToLower().Contains("type=") && _s.ToLower().Contains("&"))
                    {
                        str1 = "https://www.CVSadvantage.com/CategoryBrowse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3);
                        if (str1.Substring(str1.IndexOf("&")).IndexOf("&") > 0)
                            str1 = "https://www.CVSadvantage.com/CategoryBrowse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3, _s.Length - str1.Substring(str1.IndexOf("&")).IndexOf("&"));
                    }
                    else
                        continue;

                    if (!str1.Equals(catgBrowseCatgTmp))
                    {
                        catgBrowseCatgTmp = _s;
                        CVSCatgsFunc.AddUrl(str1);
                    }
                }
                else
                if (_s.Contains("https://www.CVSadvantage.com/browse"))
                {
                    if (_s.ToLower().Contains("id=") && _s.ToLower().Contains("type=") && _s.ToLower().Contains("&"))
                    {
                        str1 = "https://www.CVSadvantage.com/browse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3);
                        if (str1.Substring(str1.IndexOf("&")).IndexOf("&") > 0)
                            str1 = "https://www.CVSadvantage.com/browse?type=" + _s.Substring(_s.IndexOf("type=") + 5, 2) + "&Id=" + _s.Substring(_s.IndexOf("Id=") + 3, _s.Length - str1.Substring(str1.IndexOf("&")).IndexOf("&"));
                    }
                    else
                        continue;

                    if (!str1.Equals(browseTmp))
                    {
                        browseTmp = _s;
                        CVSCatgsFunc.AddUrl(str1);
                    }
                }
            }
            return true;
        }

        /* private async Task<CrawledPage> PupPageRequesterCrawler(string _uri)
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
             return await pageRequester.MakePupRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
         }*/

        /*private async Task<CrawledPage> PageRequesterCrawler(string _uri)
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
            return await pageRequester.MakePupRequestAsync(new Uri(_uri), (x) => new CrawlDecision { Allow = true }).ConfigureAwait(false);
        }*/

        /* private static PoliteWebCrawler SetupCrawler()
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
         }*/

        //private void politeWebCrawler_PageCrawlStarting(object sender, PageCrawlStartingArgs e) { }

        private void politeWebCrawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e) { }
    }
}