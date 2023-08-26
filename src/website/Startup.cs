using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CCC.website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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
                    .EnableTokenAcquisitionToCallDownstreamApi(Configuration.GetValue<string[]>("API:Scopes") ?? new string[] {string.Empty })
                    .AddDownstreamApi("API", Configuration.GetSection("API"))            
                    .AddInMemoryTokenCaches();
            //     .AddDistributedTokenCaches(); need this in app service?

            // var scopes = new string[] { "api://catcam-api/api.access"};

            // string clientSecret = Environment.GetEnvironmentVariable("AZUREAD_CLIENT_SECRET") ?? string.Empty;
            // if(string.Equals("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
            // {
            //     var _config = new ConfigurationBuilder().AddUserSecrets<Startup>().Build();
            //     clientSecret = _config.GetValue<string>("AZUREAD_CLIENT_SECRET") ?? string.Empty;
            // }
            // services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //     .AddMicrosoftIdentityWebApp(options => 
            //     {
            //         options.Instance = Configuration.GetValue<string>("AzureAd:Instance") ?? throw new Exception("Config missing AzureAd:Instance");
            //         options.Domain = Configuration.GetValue<string>("AzureAd:Domain") ?? throw new Exception("Config missing AzureAd:Domain");
            //         options.ClientId = Configuration.GetValue<string>("AzureAd:ClientId") ?? throw new Exception("Config missing AzureAd:ClientId");
            //         options.TenantId = Configuration.GetValue<string>("AzureAd:TenantId") ?? throw new Exception("Config missing AzureAd:TenantId");
            //         options.ClientSecret = clientSecret;
            //     })
            //     .EnableTokenAcquisitionToCallDownstreamApi(scopes)
            //     .AddDownstreamApi("API", Configuration.GetSection("API"))
            //     .AddInMemoryTokenCaches()
            //     .AddDistributedTokenCaches();


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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthentication();

            // From MS sample, showing how to process a token
            // app.Use(async (context, next) => {
            //     if (context != null && context.User != null && context.User.Identity.IsAuthenticated)
            //     {
            //         // you can conduct any conditional processing for guest/homes user by inspecting the value of the 'acct' claim
            //         // Read more about the 'acct' claim at aka.ms/optionalclaims
            //         if (context.User.Claims.Any(x => x.Type == "acct"))
            //         {
            //             string claimvalue = context.User.Claims.FirstOrDefault(x => x.Type == "acct").Value;
            //             string userType = claimvalue == "0" ? "Member" : "Guest";
            //             Debug.WriteLine($"The type of the user account from this Azure AD tenant is-{userType}");
            //         }
            //     }
            //     await next();
            // });


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