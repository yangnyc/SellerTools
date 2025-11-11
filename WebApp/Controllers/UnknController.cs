using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Code.Crawler.Staples;
using WebApp.Models.StaplesViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for unknown or miscellaneous inventory operations.
    /// Provides product crawling and management for unspecified retailers.
    /// </summary>
    //[System.Web.Mvc.NoAsyncTimeout]
    [Authorize]
    [Route("[controller]/[action]")]
    public class UnknController : Controller
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
        /// Displays the inventory page with current status message.
        /// </summary>
        /// <returns>The inventory view with status information.</returns>
        public IActionResult Inventory()
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
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
            return RedirectToAction("Staples");
        }

        public async Task<IActionResult> UploadCategories()
        {
            if (await new StaplesAppCrawler().PrcsStplsCats())
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
            return RedirectToAction("Staples");
        }

        public async Task<IActionResult> UploadFullProductsFromSupCat()
        {
            if (await new PupCrawl().PrcsStplsProdPerSupCatgs())
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
            return RedirectToAction(nameof(Inventory));
        }

        public async Task<IActionResult> UploadFullProductsData()
        {
            if (await new StaplesAppCrawler().ProcessStaplesProd())
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
            return RedirectToAction(nameof(Inventory));
        }

        public IActionResult ImportCatgFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc catgsFunc = new();

            if (!catgsFunc.AddUrl(_lines))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Inventory));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Inventory));
        }

        public IActionResult ImportSidCatgFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc catgsFunc = new();
            List<string[]> strList = new();

            foreach (string _s in _lines)
                strList.Add(_s.Split(","));
            if (!catgsFunc.AddSidUrl(strList))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Inventory));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Inventory));
        }

        public IActionResult ImportCatsFrXml()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.xml");
            CatgsFunc catgsFunc = new();
            DataItemCatg DataItemCatg;

            List<DataItemCatg> dataItemCatgList = new();
            foreach (string _s in _lines)
                if (_s.Contains("https://www.staples.com/"))
                {
                    DataItemCatg = new();
                    DataItemCatg.Url = _s.Replace("<loc>", " ").Replace("</loc>", " ").Trim();
                    DataItemCatg.IsCollectedHRef = false;
                    dataItemCatgList.Add(DataItemCatg);
                }
            if (!catgsFunc.Add(dataItemCatgList))
            {
                statusMessage = "Error during file upload";
                return RedirectToAction(nameof(Inventory));
            }

            statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Inventory));
        }
        public IActionResult ImportProductsFrXml()
        {
            //string[] _lines;
            //_lines = System.IO.File.ReadAllLines(@"C:\temp\a.xml");
            //ProdsFunc staplesProdsFunc = new();
            //DataItemProduct dataItemProduct;

            //List<DataItemProduct> dataItemProductList = new();
            //foreach (string _s in _lines)
            //    if (_s.Contains("https://www.staples.com/"))
            //    {
            //        dataItemProduct = new();
            //        dataItemProduct.Url = _s.Replace("<loc>", " ").Replace("</loc>", " ").Trim();
            //        dataItemProduct.Item = dataItemProduct.Url.ToLower().Substring(dataItemProduct.Url.IndexOf("product_") + 8);
            //        dataItemProductList.Add(dataItemProduct);
            //    }

            //if (!staplesProdsFunc.Add(dataItemProductList))


            //{
            //    statusMessage = "Error during file upload";
            //    return RedirectToAction(nameof(Staples));
            //}

            //statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Inventory));
        }

        public IActionResult ImportProductsFrTxt()
        {
            //string[] _lines;
            //_lines = System.IO.File.ReadAllLines(@"C:\temp\Products.txt");
            //ProdsFunc staplesProdsFunc = new();
            //DataItemProduct dataItemProduct;

            //List<DataItemProduct> dataItemProductList = new();
            //foreach (string _s in _lines)
            //    if (_s.Contains("https://www.staples.com/"))
            //    {
            //        dataItemProduct = new();
            //        dataItemProduct.Url = _s.Substring(_s.IndexOf("|||") + 3);//.Replace("staples", "staplesadvantage");
            //        dataItemProduct.Item = _s.Substring(0, _s.IndexOf("|||"));
            //        dataItemProductList.Add(dataItemProduct);
            //    }

            //if (!staplesProdsFunc.Add(dataItemProductList))
            //{
            //    statusMessage = "Error during file upload";
            //    return RedirectToAction(nameof(Staples));
            //}

            //statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(Inventory));
        }

        public IActionResult ExportProductsToTxt()
        {
            List<String> strList = new();
            ProdsFunc staplesProdsFunc = new();
            List<DataItemProduct> dataItemProductList = new();

            dataItemProductList = staplesProdsFunc.GetAll();

            foreach (DataItemProduct _s in dataItemProductList)
            {
                strList.Add(_s.Item + "|||" + _s.Url);
            }

            System.IO.File.WriteAllLines(@"C:\temp\Products1.txt", strList);
            statusMessage = "Export Completed";

            return RedirectToAction(nameof(Inventory));
        }

        public async Task<IActionResult> GetFromGoogleCache()
        {

            if (await new StaplesAppCrawler().FindGoogleCache())
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";

            return RedirectToAction(nameof(Inventory));
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
