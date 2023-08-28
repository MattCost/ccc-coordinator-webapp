using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CCC.website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            services.AddOptions();

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, Constants.AzureAdB2C)
                    // .EnableTokenAcquisitionToCallDownstreamApi( Configuration.GetValue<List<string>>("API:Scopes") ?? throw new Exception("API:Scopes is required!") ) //ask for the scope right away
                    .EnableTokenAcquisitionToCallDownstreamApi()    // don't ask for the scope now.
                    .AddDownstreamApi("API", options =>
                    {
                        options.BaseUrl = "https://app-ccc-webapp-api-dev.azurewebsites.net/api/";
                        options.Scopes = new List<string> { "https://cccwebapp.onmicrosoft.com/ccc-webapp-api/API.Access" };
                    })
                    // For some reason the list of strings is not working in appsettings.json with this configuration. no idea why
                    // .AddDownstreamApi("API", Configuration.GetSection("API"))            
                    .AddInMemoryTokenCaches();

            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting.  For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages();

            services.Configure<OpenIdConnectOptions>(Configuration.GetSection(Constants.AzureAdB2C));

            services.Configure<CookieAuthenticationOptions>(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.Events = new RejectStaleSessionCookie();
                }
            );

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

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
        }
    }
}