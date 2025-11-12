using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLDBApp.Funcs;
using SQLDBApp.Models;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using WebApp.Code.Crawler.Adv;
using WebApp.Code.Crawler.CVS;
using WebApp.Misc;
using WebApp.Models.CVSViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for CVS pharmacy data crawling and management operations.
    /// Handles category imports, product data extraction, and Excel exports.
    /// </summary>
    //[System.Web.Mvc.NoAsyncTimeout]
    [Authorize]
    [Route("[controller]/[action]")]
    public class CVSController : Controller
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
        /// Displays the CVS operations page with current status message.
        /// </summary>
        /// <returns>The CVS view with status information.</returns>
        public IActionResult CVS()
        {
            return View(ModelFromStatusMessage());
        }

        /// <summary>
        /// Imports category URLs from a text file (C:\temp\a.txt).
        /// </summary>
        /// <returns>Redirects to CVS view with success or error message.</returns>
        public IActionResult ImportCatgsFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc catgsFunc = new();

            if (!catgsFunc.AddUrl(_lines))
                statusMessage = "Error during file upload";
            else
                statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Imports category data with IDs from a comma-separated text file (C:\temp\a.txt).
        /// </summary>
        /// <returns>Redirects to CVS view with success or error message.</returns>
        public IActionResult ImportSidCatgsFrTxt()
        {
            string[] _lines;
            _lines = System.IO.File.ReadAllLines(@"C:\temp\a.txt");
            CatgsFunc catgsFunc = new();
            List<string[]> strList = new();

            foreach (string _s in _lines)
                strList.Add(_s.Split(","));
            if (!catgsFunc.AddSidUrl(strList))
                statusMessage = "Error during file upload";
            else
                statusMessage = "File Uploaded Successfully";
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Imports categories from CVS sitemap XML file.
        /// Crawls the XML, extracts category URLs, and saves them to the database.
        /// </summary>
        /// <returns>Redirects to CVS view with success or error message.</returns>
        public async Task<IActionResult> ImportCatsFrXml()
        {
            string url = @"https://www.cvs.com/sitemap/shop/sitemap_category.xml";
            IHtmlDocument angleSharp = await new CVSPupCrawl().GetUrlAngleSharp(url);
            if (angleSharp == null)
                return SendErrorMsg("Get Html from " + url + " returned error");
            var loc = angleSharp.QuerySelectorAll("loc");
            CatgsFunc catgsFunc = new();
            DataItemCatg dataItemCatg;
            List<DataItemCatg> dataItemCatgList = new();
            foreach (IElement a in loc)
            {
                if (a.InnerHtml.Contains(@"www.cvs.com"))
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
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Parses category URLs and fills the C1-C10 fields with URL path segments.
        /// Also sets the category name from the last URL segment.
        /// </summary>
        /// <returns>Redirects to CVS view or error message.</returns>
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
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Exports all category data to an Excel file.
        /// </summary>
        /// <returns>Redirects to CVS view with file creation status.</returns>
        public IActionResult ExportCatgsToExcel()
        {
            string fileName = new OutputDir().GetNewFileName("xlsx");
            if (new ExcelImpExp().WriteDataTableToExcel(new CatgsFunc().GetDataTable(), "Catgs", fileName))
                statusMessage = "Excel created " + fileName;
            else
                statusMessage = "Excel export error " + fileName;
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Uploads all product prices by crawling advertisement product tables.
        /// </summary>
        /// <returns>Redirects to index with operation status.</returns>
        public async Task<IActionResult> UploadProdPricesAll()
        {
            SetMessage(await (new PupCrawlAdv()).PrcsAdvProdTables());
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sets an error message and redirects to CVS view.
        /// </summary>
        /// <param name="msg">The error message to display.</param>
        /// <returns>Redirects to CVS view.</returns>
        public IActionResult SendErrorMsg(string msg)
        {
            statusMessage = msg;
            return RedirectToAction(nameof(CVS));
        }

        /// <summary>
        /// Sets the status message based on operation result.
        /// </summary>
        /// <param name="result">True if operation succeeded, false otherwise.</param>
        private void SetMessage(bool result)
        {
            if (result)
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
        }

        /// <summary>
        /// Creates a view model from the current status message.
        /// </summary>
        /// <returns>CVSViewModel with status message.</returns>
        private CVSViewModel ModelFromStatusMessage()
        {
            CVSViewModel CVSViewModel = new() { statusMessage = statusMessage };
            if (!string.IsNullOrEmpty(statusMessage))
                CVSViewModel.statusMessage = statusMessage;
            return CVSViewModel;
        }

        /*public async Task<IActionResult> GetTitlePriceByCVSModelNum()
        {
            if (await new PupCrawl().GetTitlePriceByCVSModelNum())
                if (await new CVSAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("CVS");
        }*/
        /* public async Task<IActionResult> UploadCategories()
          {
              if (await new CVSAppCrawler().PrcsStplsCats())
                  if (await new CVSAppCrawler().FindGoogleCache())
                      SetMessage(true);
                  else
                      SetMessage(false);
              return RedirectToAction("CVS");
          }*/
        /* public async Task<IActionResult> UploadFullProductsFromSupCat()
         {
             if (await new PupCrawl().PrcsStplsProdPerSupCatgs())
                 if (await new CVSAppCrawler().FindGoogleCache())
                     SetMessage(true);
                 else
                     SetMessage(false);
             return RedirectToAction(nameof(CVS));
         }*/
        /*public async Task<IActionResult> UploadFullProductsData()
        {
            if (await new CVSAppCrawler().ProcessCVSProd())
                SetMessage(true);
            else
                SetMessage(false);
            return RedirectToAction(nameof(CVS));
        }*/
        /* public IActionResult ImportProductsFrTxt()
         {
             string[] _lines;
             _lines = System.IO.File.ReadAllLines(@"C:\temp\Products.txt");
             CVSProdsFunc CVSProdsFunc = new();
             CVSProducts CVSProducts;

             List<CVSProducts> CVSProductsList = new();
             foreach (string _s in _lines)
                 if (_s.Contains("https://www.CVS.com/"))
                 {
                     CVSProducts = new();
                     CVSProducts.Url = _s.Substring(_s.IndexOf("|||") + 3);//.Replace("CVS", "CVSadvantage");
                     CVSProducts.Item = _s.Substring(0, _s.IndexOf("|||"));
                     CVSProductsList.Add(CVSProducts);
                 }

             if (!CVSProdsFunc.Add(CVSProductsList))
             {
                 StatusMessage = "Error during file upload";
                 return RedirectToAction(nameof(CVS));
             }

             StatusMessage = "File Uploaded Successfully";
             return RedirectToAction(nameof(CVS));
         }*/
        /*public IActionResult ExportProductsToTxt()
        {
            List<String> strList = new();
            CVSProdsFunc CVSProdsFunc = new();
            List<CVSProducts> CVSProductsList = new();

            CVSProductsList = CVSProdsFunc.GetAll();

            foreach (CVSProducts _s in CVSProductsList)
            {
                strList.Add(_s.Item + "|||" + _s.Url);
            }

            System.IO.File.WriteAllLines(@"C:\temp\Products1.txt", strList);
            StatusMessage = "Export Completed";

            return RedirectToAction(nameof(CVS));
        }*/
    }
}
