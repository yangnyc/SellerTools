using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using WebApp.Code.Crawler.Adv;
using WebApp.Code.Crawler.CVS;
using WebApp.Code.Crawler.Staples;
using WebApp.Models.StaplesViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for Staples retail data crawling and management operations.
    /// Handles category imports, product data extraction, and inventory management.
    /// </summary>
    //[System.Web.Mvc.NoAsyncTimeout]
    [Authorize]
    [Route("[controller]/[action]")]
    public class StaplesController : Controller
    {
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        [TempData]
        public string statusMessage { get; set; } = "";

        /// <summary>
        /// Displays the main index page.
        /// </summary>
        /// <returns>The index view.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Debug function for testing product set operations.
        /// </summary>
        /// <returns>Redirects to Staples view.</returns>
        public IActionResult TestDebugFunc1()
        {
            (new ProdsFunc()).Set(new DataItemProduct());
            return RedirectToAction("Staples");
        }

        /// <summary>
        /// Displays the Staples operations page with current status message.
        /// </summary>
        /// <returns>The Staples view with status information.</returns>
        public IActionResult Staples()
        {
            return View(ModelFromStatusMessage());
        }

        /// <summary>
        /// Retrieves product title and price using Staples model number.
        /// </summary>
        /// <returns>Redirects to Staples view with operation status.</returns>
        public async Task<IActionResult> GetTitlePriceByStaplesModelNum()
        {
            if (await new PupCrawl().GetTitlePriceByStaplesModelNum())
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Staples");
        }

        public async Task<IActionResult> UploadCategories()
        {
            if (await new StaplesAppCrawler().PrcsStplsCats())
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Staples");
        }

        public async Task<IActionResult> UploadFullProductsFromSupCat()
        {
            if (await new PupCrawl().PrcsStplsProdPerSupCatgs())
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction(nameof(Staples));
        }

        public async Task<IActionResult> UploadFullProductsData()
        {
            if (await new StaplesAppCrawler().ProcessStaplesProd())
                SetMessage(true);
            else
                SetMessage(false);
            return RedirectToAction(nameof(Staples));
        }

        public IActionResult ImportCatgsFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc catgsFunc = new();

            if (!catgsFunc.AddUrl(_lines))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Staples));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Staples));
        }

        public IActionResult ImportSidCatgsFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc staplesCatgsFunc = new();
            List<string[]> strList = new();

            foreach (string _s in _lines)
                strList.Add(_s.Split(","));
            if (!staplesCatgsFunc.AddSidUrl(strList))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Staples));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Staples));
        }

        public async Task<IActionResult> ImportCatsFrXml()
        {
            string url = @"https://www.staples.com/sitemap/staples-sitemap-www-staples-com-en_us-category-1.xml.gz";
            IHtmlDocument angleSharp = await new CVSPupCrawl().GetUrlAngleSharp(url);
            if (angleSharp == null)
                return SendErrorMsg("Get Html from " + url + " returned error");
            var loc = angleSharp.QuerySelectorAll("loc");
            CatgsFunc catgsFunc = new();
            DataItemCatg dataItemCatg;
            List<DataItemCatg> dataItemCatgList = new();
            foreach (IElement a in loc)
            {
                if (a.InnerHtml.Contains(@"www.staples.com"))
                {
                    if (catgsFunc.IsExist(a.InnerHtml))
                        continue;
                    dataItemCatg = new();
                    dataItemCatg.Url = a.InnerHtml.Trim();
                    dataItemCatg.IsCollectedHRef = false;
                    dataItemCatgList.Add(dataItemCatg);
                }
            }
            if (!catgsFunc.Add(dataItemCatgList))
                return SendErrorMsg("Error during file upload");
            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Staples));
        }
        public IActionResult FillC1toC10()
        {
            CatgsFunc catgsFunc = new();
            List<DataItemCatg> dataItemCatgsList = new();
            dataItemCatgsList = catgsFunc.GetAll();
            string[] stringList;

            foreach (DataItemCatg dataItemCatg in dataItemCatgsList)
            {
                stringList = dataItemCatg.Url.Split('/');
                int i = 0;
                foreach (string s in stringList)
                {
                    if (i < 4) { i++; continue; }
                    if (i == 4) { dataItemCatg.C1=s; i++; continue; }
                    if (i == 5) { dataItemCatg.C2=s; i++; continue; }
                    if (i == 6) { dataItemCatg.C3=s; i++; continue; }
                    if (i == 7) { dataItemCatg.C4=s; i++; continue; }
                    if (i == 8) { dataItemCatg.C5=s; i++; continue; }
                    if (i == 9) { dataItemCatg.C6=s; i++; continue; }
                    if (i == 10) { dataItemCatg.C7=s; i++; continue; }
                    return SendErrorMsg("ERROR");
                }
                dataItemCatg.Name = stringList[i-1];
                catgsFunc.Set(dataItemCatg);
            }
            return RedirectToAction(nameof(Staples));
        }
        public IActionResult ImportProductsFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\Products.txt");
            ProdsFunc staplesProdsFunc = new();
            DataItemProduct dataItemProduct;

            List<DataItemProduct> dataItemProductsList = new();
            foreach (string _s in _lines)
                if (_s.Contains("https://www.staples.com/"))
                {
                    dataItemProduct = new();
                    dataItemProduct.Url = _s.Substring(_s.IndexOf("|||") + 3);//.Replace("staples", "staplesadvantage");
                    dataItemProduct.Item = _s.Substring(0, _s.IndexOf("|||"));
                    dataItemProductsList.Add(dataItemProduct);
                }

            if (!staplesProdsFunc.Add(dataItemProductsList))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Staples));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Staples));
        }

        public IActionResult ExportProductsToTxt()
        {
            List<String> strList = new();
            ProdsFunc staplesProdsFunc = new();
            List<DataItemProduct> dataItemProductsList = new();

            dataItemProductsList = staplesProdsFunc.GetAll();

            foreach (DataItemProduct _s in dataItemProductsList)
            {
                strList.Add(_s.Item + "|||" + _s.Url);
            }

            System.IO.File.WriteAllLines(@"C:\temp\Products1.txt", strList);
            statusMessage = "Export Completed";

            return RedirectToAction(nameof(Staples));
        }

        public async Task<IActionResult> GetFromGoogleCache()
        {

            if (await new StaplesAppCrawler().FindGoogleCache())
                SetMessage(true);
            else
                SetMessage(false);
            return RedirectToAction(nameof(Staples));
        }

        public IActionResult StaplesAdv()
        {
            return View(ModelFromStatusMessage());
        }

        //StaplesAdv
        public async Task<IActionResult> UploadProdPricesAll()
        {
            SetMessage(await (new PupCrawlAdv()).PrcsAdvProdTables());
            return RedirectToAction("Index");
        }

        public IActionResult SendErrorMsg(string msg)
        {
            statusMessage = msg;
            return RedirectToAction(nameof(Staples));
        }

        private void SetMessage(bool result)
        {
            if (result)
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
        }

        private StaplesViewModel ModelFromStatusMessage()
        {
            StaplesViewModel staplesViewModel = new() { statusMessage = statusMessage };
            if (!string.IsNullOrEmpty(statusMessage))
                staplesViewModel.statusMessage = statusMessage;
            return staplesViewModel;
        }
    }
}
