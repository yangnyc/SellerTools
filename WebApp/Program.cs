using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Office.Interop.Excel;
using System.Net.Sockets;
using WebApp.Code.Crawler;

namespace WebApp
{
    /// <summary>
    /// Main entry point for the ASP.NET Core web application.
    /// Configures and starts the web host with Kestrel server settings.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point. Creates and runs the web host.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates and configures the web host builder.
        /// Configures Kestrel server options and uses Startup class for service configuration.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Configured IHostBuilder instance.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => { services.Configure<KestrelServerOptions>(context.Configuration.GetSection("Kestrel")); })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}