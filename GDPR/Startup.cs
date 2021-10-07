using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GDPR
{
    public class Startup
    {
        /// <summary>
        /// When the application is requested for the first time, it calls ConfigureServices method. 
        /// ASP.net core has built-in support for Dependency Injection. We can add services to DI container using this method.
        /// Use ConfigureServices method to configure Dependency Injection (services).
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<CookiePolicyOptions>(options =>
                {
                    // The CheckConsentNeeded option of true will prevent any non-essential cookies 
                    // from being sent to the browser (no Set-Cookie header) without the user's explicit permission.
                    // You can either change this behaviour, or mark your cookie as essential by setting the 
                    // IsEssential property to true when creating it: options.Cookie.IsEssential = true;
                    options.CheckConsentNeeded = context => true;       // Enable the default cookie consent feature                                                                     
                    options.MinimumSameSitePolicy = SameSiteMode.None;  // Requires using Microsoft.AspNetCore.Http;
                    options.HttpOnly = HttpOnlyPolicy.Always;
                    options.Secure = CookieSecurePolicy.SameAsRequest;
                });


            IMvcBuilder mvcBuilder = services.AddControllersWithViews()
                /// Use session based TempData instead of cookie based TempData
                .AddSessionStateTempDataProvider();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                /// Install-Package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation   
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            services.AddRazorPages();

            services.AddHttpContextAccessor();

            // The IDistributedCache implementation is used as a backing store for session
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // A simple Authentication scenario
            services
                // .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login/";
                    options.LogoutPath = "/Account/Logout/";
                });
        }

        /// <summary>
        /// This method is used to define how the application will respond on each HTTP request.
        /// This method is also used to configure middleware in HTTP pipeline.
        /// Use Configure method to set up middlewares, routing rules etc.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                // specific route should be created before the generic route
                endpoints.MapGet("/license", async context => { await context.Response.WriteAsync(System.IO.File.ReadAllText("license.md")); });
                endpoints.MapAreaControllerRoute(name: "admin", areaName: "admin", pattern: "admin/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(name: "areaRoute", pattern: "{area:exists}/{controller}/{action}");
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Service was unable to handle this request.");
            });
        }
    }
}

// ------------------------------------------------------------------------------------------------------------
// The following Startup.Configure method adds security-related middleware components in the recommended order:
// ------------------------------------------------------------------------------------------------------------

//public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//{
//    if (env.IsDevelopment()) {
//        app.UseDeveloperExceptionPage();
//        app.UseDatabaseErrorPage();
//    }
//    else {
//        app.UseExceptionHandler("/Error");
//        app.UseHsts();
//    }
//    app.UseHttpsRedirection();
//    app.UseStaticFiles();
//    app.UseCookiePolicy();
//    app.UseRouting();
//    app.UseRequestLocalization();
//    app.UseCors();
//    app.UseAuthentication();
//    app.UseAuthorization();
//    app.UseSession();
//    app.UseResponseCaching();
//    app.UseEndpoints(endpoints =>
//    {
//        endpoints.MapRazorPages();
//        endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
//    });
//}