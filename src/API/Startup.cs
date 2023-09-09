
using System.IdentityModel.Tokens.Jwt;
using Azure.Identity;
using CCC.Services.EntityProvider;
using CCC.Services.Secrets;
using CCC.Services.UserProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;

namespace CCC.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureCommonServices(IServiceCollection services)
        {
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting.  For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);
                    },
                        options => { Configuration.Bind("AzureAdB2C", options); });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Common.Authorization.Enums.CoordinatorAdminPolicy,
                    policy => policy.RequireClaim(Common.Authorization.Enums.IsCoordinatorAdminClaim, new string[] { "true" }));

                options.AddPolicy(Common.Authorization.Enums.CoordinatorPolicy,
                    policy => policy.RequireClaim(Common.Authorization.Enums.IsCoordinatorClaim, new string[] { "true" }));

                options.AddPolicy(Common.Authorization.Enums.ContributorPolicy,
                    policy => policy.RequireClaim(Common.Authorization.Enums.IsContributorClaim, new string[] { "true" }));

                options.AddPolicy(Common.Authorization.Enums.AdminPolicy,
                    policy => policy.RequireClaim(Common.Authorization.Enums.IsAdminClaim, new string[] { "true" }));

                //Basically a dupe of fallback policy
                options.AddPolicy(Common.Authorization.Enums.ReadOnlyPolicy,
                    policy => policy.RequireAuthenticatedUser());

                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            var msGraphScopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                Configuration.GetValue<string>("AzureAdB2C:Domain"),
                Configuration.GetValue<string>("AzureAdB2C:ClientId"),
                Configuration.GetValue<string>("AzureAdB2C:ClientSecret"));
            var graphClient = new GraphServiceClient(clientSecretCredential, msGraphScopes);

            services.AddSingleton(graphClient);
            services.AddMvc(options =>
            {
                // options.Filters.Add<CustomExceptionFilter>();
            });

            services.AddControllers();

            services.AddEndpointsApiExplorer();

            services.AddAzureAdB2CProtectedSwagger(options =>
            {
                options.B2CDomain = Configuration.GetValue<string>("Swagger:B2CDomain") ?? string.Empty;
                options.PolicyName = Configuration.GetValue<string>("Swagger:PolicyId") ?? "B2C_1_SignupSignin";
                options.ClientId = Configuration.GetValue<Guid>("Swagger:ClientId");
                options.Scopes = new Dictionary<string, string>
                {
                    [Configuration.GetValue<string>("Swagger:Scope") ?? "scope"] = Configuration.GetValue<string>("Swagger:ScopeDisplay") ?? "empty"
                };
                // This didn't work in the app service, even tho the config entry looked the same as appsettings entry
                // options.Scopes = Configuration.GetSection("Swagger:Scopes").GetChildren().ToDictionary( x => $"https://{x.Key}", x => x.Value ?? string.Empty);
            });
            services.AddHttpClient();

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("coordinatorRole", typeof(CoordinatorRoleConstraint));
            });

        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            // services.AddSingleton<IEntityProvider, MemoryEntityProvider>();
            services.AddSingleton<IEntityProvider, EntityProviderTableStorage>();
            services.AddSingleton<ISecretsManager, UserSecretsSecretManager>();
            // services.AddSingleton<IAuthorizationHandler, AlwaysAllowAuthHandler>();
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            services.AddSingleton<IEntityProvider, EntityProviderTableStorage>();
            services.AddSingleton<IUserProvider, GraphAPIUserProvider>();
            services.AddSingleton<ISecretsManager, EnvVarSecretManager>();
            // Auth handlers after app roles are defined and worked out
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                // dev certs don't work on fedora. need to do manual setup to use https locally, so when in development, we don't use https
                app.UseHttpsRedirection();
            }


            app.AddSwaggerUi(options =>
            {
                options.ClientId = Configuration.GetValue<Guid>("Swagger:ClientId");
                options.Scopes = new Dictionary<string, string>
                {
                    [Configuration.GetValue<string>("Swagger:Scope") ?? "scope"] = Configuration.GetValue<string>("Swagger:ScopeDisplay") ?? "empty"
                };

                // This didn't work in app service.
                // options.Scopes = Configuration.GetSection("Swagger:Scopes").GetChildren().ToDictionary( x => $"https://{x.Key}", x => x.Value ?? string.Empty);
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}