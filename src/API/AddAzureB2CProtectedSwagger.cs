using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection;

public class AuthUrlSettings
{
    public string B2CDomain { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Dictionary<string, string> Scopes { get; set; } = new();

    public string AuthorizationUrl => $"https://{B2CDomain}.b2clogin.com/{B2CDomain}.onmicrosoft.com/oauth2/v2.0/authorize?p={PolicyName}";
    public string TokenUrl => $"https://login.microsoftonline.com/{B2CDomain}.onmicrosoft.com/oauth2/v2.0/token?p={PolicyName}";
}

public static class AddMySwaggerGenCollectionExtensions
{
    public static IApplicationBuilder AddSwaggerUi(this IApplicationBuilder app, Action<AuthUrlSettings> setupAction)
    {
        var settings = new AuthUrlSettings();
        setupAction(settings);

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnableTryItOutByDefault();
            options.OAuthUsePkce();
            options.OAuthClientId(settings.ClientId.ToString());
            options.OAuthScopes(settings.Scopes.Keys.ToArray());
            
        });

        return app;
    }
    public static IServiceCollection AddAzureAdB2CProtectedSwagger(this IServiceCollection services, Action<AuthUrlSettings> setupAction)
    {
        var settings = new AuthUrlSettings();
        setupAction(settings);

        services.AddSwaggerGen(config =>
        {
            config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "AzureAd B2C Login",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(settings.AuthorizationUrl),
                        TokenUrl = new Uri(settings.TokenUrl),
                        Scopes = settings.Scopes
                    }
                }
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { settings.Scopes.First().Value } //WWhat does this do?
                }                
            });
        });

        return services;
    }
}
/*

                      
*/