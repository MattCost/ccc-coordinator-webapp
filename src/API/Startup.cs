
using System.IdentityModel.Tokens.Jwt;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //         .AddMicrosoftIdentityWebApi(options =>
            // {
            //     Configuration.Bind("AzureAdB2C", options);
            // },
            // options => { Configuration.Bind("AzureAdB2C", options); });
        
            services.AddMvc(options =>
            {
                // options.Filters.Add<CustomExceptionFilter>();
            });

            services.AddControllers();

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen();
            services.AddHttpClient();

        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            services.AddSingleton<IEntityProvider, MemoryEntityProvider>();
            // services.AddSingleton<ISecretsManager, UserSecretsSecretManager>();
            // services.AddSingleton<IAuthorizationHandler, AlwaysAllowAuthHandler>();
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            services.AddSingleton<IEntityProvider, MemoryEntityProvider>();
            // services.AddSingleton<ISecretsManager, EnvVarSecretManager>();
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
                // app.UseSwagger();
                // app.UseSwaggerUI(options =>
                // {
                //     options.EnableTryItOutByDefault();
                //     options.OAuthUsePkce();
                //     options.OAuthClientId("a6fe10f9-9e44-4865-8ee8-ccacca8715f1");
                //     options.OAuthScopes(new string[] { "api://catcam-api/api.access" });
                // });
            }
            else
            {
                app.UseHsts();
                // dev certs don't work on fedora. need to do manual setup to use https locally
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();

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