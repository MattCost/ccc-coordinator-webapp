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
            // services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                // options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            services.AddOptions();

            var initialScope = Configuration.GetValue<string>("API:Scope") ?? throw new Exception("API:Scope is required");

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, Constants.AzureAdB2C)
                    //Since we only have 1 downstream scope, we can request it now. 
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { initialScope })
                    .AddDownstreamApi("API", options =>
                    {
                        options.BaseUrl = Configuration.GetValue<string>("API:BaseUrl");
                        options.Scopes = new string[] { initialScope };
                    })
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

            services.AddAuthorization(options =>
            {
                options.AddPolicy(CCC.Authorization.Enums.AdminPolicy,
                    policy => policy.RequireClaim(CCC.Authorization.Enums.IsAdminClaim, new string[] { "true" }));

                options.AddPolicy(CCC.Authorization.Enums.CoordinatorAdminPolicy,
                    policy => policy.RequireAssertion(context => context.User.HasClaim(c => (c.Type == CCC.Authorization.Enums.IsAdminClaim || c.Type == CCC.Authorization.Enums.IsCoordinatorAdminClaim) && c.Value == "true")));

                options.AddPolicy(CCC.Authorization.Enums.CoordinatorPolicy,
                    policy => policy.RequireClaim(CCC.Authorization.Enums.IsCoordinatorClaim, new string[] { "true" }));

                options.AddPolicy(CCC.Authorization.Enums.ContributorPolicy,
                    policy => policy.RequireAssertion(context => context.User.HasClaim(c => (c.Type == CCC.Authorization.Enums.IsAdminClaim || c.Type == CCC.Authorization.Enums.IsContributorClaim) && c.Value == "true")));

                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });


            services.AddRazorPages(option =>
            {
                option.Conventions.AuthorizePage("/BikeRoutes/Create", CCC.Authorization.Enums.ContributorPolicy);
                option.Conventions.AuthorizePage("/BikeRoutes/Edit", CCC.Authorization.Enums.ContributorPolicy);
            }).AddSessionStateTempDataProvider();

            services.AddSession();

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
            app.UseSession();

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