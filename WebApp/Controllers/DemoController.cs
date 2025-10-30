using AngleSharp.Io;
using Devweb.Core;
using Devweb.Poco;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLDBApp.Funcs;
using SQLDBApp.Models.DataItems;
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
            Uri _uri = new Uri(@"https://www.gmail.com/");

            await new DemoPupCrawl().GetUrlHtml(_uri.ToString());

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
