using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Code.Crawler.Demo;
using WebApp.Code.Crawler.Staples;
using WebApp.Models.DemoViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// Demo controller for testing web crawling functionality.
    /// Provides test actions for Puppeteer-based web scraping operations.
    /// </summary>
    //[System.Web.Mvc.NoAsyncTimeout]
    [Authorize]
    [Route("[controller]/[action]")]
    public class DemoController : Controller
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
        /// Displays the demo page with current status message.
        /// </summary>
        /// <returns>The demo view with status information.</returns>
        public IActionResult Demo()
        {
            return View(ModelFromStatusMessage());
        }

        /// <summary>
        /// Tests Chromium browser automation by crawling a test URL.
        /// </summary>
        /// <returns>Redirects to Demo view with operation status.</returns>
        public async Task<IActionResult> RunChromium()
        {
            if (await new DemoPupCrawl().CrawlUrl(new Uri(@"https://www.otcsuperstore.com/")))
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Demo");
        }

        /// <summary>
        /// Tests URL reading functionality by fetching HTML from a test URL.
        /// </summary>
        /// <returns>Redirects to Demo view with crawling status.</returns>
        public async Task<IActionResult> ReadUrl()
        {
            Uri uri = new(@"https://www.gmail.com/");
            string? result = await new DemoPupCrawl().GetUrlHtml(uri);
            if (result == null)
                statusMessage = "Crawling " + uri.ToString() + " was not successful";
            statusMessage = "Crawling " + uri.ToString() + " was successful";

            return RedirectToAction("Demo");
        }

        /// <summary>
        /// Tests category selection functionality on a demo site.
        /// </summary>
        /// <returns>Redirects to Demo view with operation status.</returns>
        public async Task<IActionResult> ChooseVitaminsCat()
        {
            if (await new DemoPupCrawl().CrawlUrl(new Uri(@"https://www.otcsuperstore.com/dsdsds/")))
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Demo");
        }

        /// <summary>
        /// Sets an error message and redirects to Demo view.
        /// </summary>
        /// <param name="msg">The error message to display.</param>
        /// <returns>Redirects to Demo view.</returns>
        public IActionResult SendErrorMsg(string msg)
        {
            statusMessage = msg;
            return RedirectToAction(nameof(Demo));
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
        /// <returns>DemoViewModel with status message.</returns>
        private DemoViewModel ModelFromStatusMessage()
        {
            DemoViewModel DemoViewModel = new() { statusMessage = statusMessage };
            if (!string.IsNullOrEmpty(statusMessage))
                DemoViewModel.statusMessage = statusMessage;
            return DemoViewModel;
        }
    }
}
