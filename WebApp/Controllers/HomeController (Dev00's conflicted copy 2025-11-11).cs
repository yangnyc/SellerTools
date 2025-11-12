using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebApp.Models;

namespace WebApp.Controllers
{
    /// <summary>
    /// Main controller for the application home page and error handling.
    /// Manages the landing page and displays error information.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Logger instance for tracking application events.
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">Logger for this controller.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the home page with transparent header styling.
        /// </summary>
        /// <returns>The home index view.</returns>
        public IActionResult Index()
        {
            ViewData["MainPage"] = "header-transparent";
            ViewData["MainPageNav"] = "bg-transparent";
            ViewData["MainPageTxt"] = "text-white";

            return View();
        }

        /// <summary>
        /// Displays the error page with request tracking information.
        /// </summary>
        /// <returns>The error view with diagnostic details.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
