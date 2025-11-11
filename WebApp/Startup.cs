using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.EntityFrameworkCore;
using SQLDBApp.Data;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

namespace WebApp
{
    /// <summary>
    /// Configures services and the HTTP request pipeline for the web application.
    /// Sets up database contexts, identity authentication, and server options.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration settings.</param>
        public Startup(IConfiguration configuration) => Configuration = configuration;

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// Configures services for dependency injection.
        /// Sets up database contexts, identity, authentication, and server configuration.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("SQLDBApp")));
            services.AddDbContext<DevSqlDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DevSqDBContext")));
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.Configure<FormOptions>(options =>
           {
               options.ValueLengthLimit = int.MaxValue;
               options.MultipartBodyLengthLimit = int.MaxValue;
               options.MemoryBufferThreshold = int.MaxValue;
           }
            );

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(150);
                // If the LoginPath isn't set, ASP.NET Core defaults 
                // the path to /Account/Login.
                options.LoginPath = "/Account/Login";
                // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
                // the path to /Account/AccessDenied.
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.Limits.KeepAliveTimeout = TimeSpan.FromDays(2);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
                options.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromDays(2);
                options.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromDays(2);
                options.Limits.Http2.MaxStreamsPerConnection = 100;
                options.Limits.MaxConcurrentConnections = null;
                options.Limits.MinRequestBodyDataRate = null;
                options.Limits.MinResponseDataRate = null;
            });

            //// Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddControllersWithViews();
            services.AddMvc();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the HTTP request pipeline.
        /// Sets up middleware for error handling, HTTPS, static files, routing, and authentication.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.Run(async (context) =>
            {
                await System.Console.Out.WriteAsync("Started: " + DateTime.Now);
                context.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = 100_000_000;
                var minRequestRateFeature = context.Features.Get<IHttpMinRequestBodyDataRateFeature>();
                var minResponseRateFeature = context.Features.Get<IHttpMinResponseDataRateFeature>();
                if (minRequestRateFeature != null)
                    minRequestRateFeature = null;
                if (minResponseRateFeature != null)
                    minResponseRateFeature = null;
            });
        }
    }
}