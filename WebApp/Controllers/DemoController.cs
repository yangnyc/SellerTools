using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Code.Crawler.Demo;
using WebApp.Code.Crawler.Staples;
using WebApp.Models.DemoViewModels;

namespace WebApp.Controllers
{
    //[System.Web.Mvc.NoAsyncTimeout]
    [Authorize]
    [Route("[controller]/[action]")]
    public class DemoController : Controller
    {
        [TempData]
        public string statusMessage { get; set; } = "";

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Demo()
        {
            return View(ModelFromStatusMessage());
        }

        public async Task<IActionResult> RunChromium()
        {
            if (await new DemoPupCrawl().CrawlUrl(new Uri(@"https://www.otcsuperstore.com/")))
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Demo");
        }
        public async Task<IActionResult> ReadUrl()
        {
            Uri uri = new(@"https://www.gmail.com/");
            string? result = await new DemoPupCrawl().GetUrlHtml(uri);
            if (result == null)
                statusMessage = "Crawling " + uri.ToString() + " was not successful";
            statusMessage = "Crawling " + uri.ToString() + " was successful";

            return RedirectToAction("Demo");
        }

        public async Task<IActionResult> ChooseVitaminsCat()
        {
            if (await new DemoPupCrawl().CrawlUrl(new Uri(@"https://www.otcsuperstore.com/dsdsds/")))
                if (await new StaplesAppCrawler().FindGoogleCache())
                    SetMessage(true);
                else
                    SetMessage(false);
            return RedirectToAction("Demo");
        }

        public IActionResult SendErrorMsg(string msg)
        {
            statusMessage = msg;
            return RedirectToAction(nameof(Demo));
        }

        private void SetMessage(bool result)
        {
            if (result)
                statusMessage = "Completed Successfully";
            else
                statusMessage = "Completed with Errors";
        }

        private DemoViewModel ModelFromStatusMessage()
        {
            DemoViewModel DemoViewModel = new() { statusMessage = statusMessage };
            if (!string.IsNullOrEmpty(statusMessage))
                DemoViewModel.statusMessage = statusMessage;
            return DemoViewModel;
        }
    }
}
