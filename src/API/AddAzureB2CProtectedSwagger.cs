using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection;

public class AuthUrlSettings
{
    public string B2CDomain {get;set; } = string.Empty;
    public string PolicyName {get;set;} = string.Empty;
    public Guid ClientId {get;set;}
    public string RedirectUrl {get;set;} = string.Empty;
    public Dictionary<string, string> Scopes {get;set;} = new();

    public string AuthorizationUrl => $"https://{B2CDomain}.b2clogin.com/{B2CDomain}.onmicrosoft.com/oauth2/v2.0/authorize?p={PolicyName}&response_type=code&client_id={ClientId}&redirect_uri={RedirectUrl}";
    public string TokenUrl => $"https://login.microsoftonline.com/{B2CDomain}.onmicrosoft.com/oauth2/v2.0/token";

}

public static class AddMySwaggerGenCollectionExtensions
{
    public static IServiceCollection AddAzureAdB2CProtectedSwagger(this IServiceCollection services, Action<AuthUrlSettings>? setupAction = null)
    {
        var settings = new AuthUrlSettings();
        if (setupAction != null)
            setupAction(settings);


        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            In = ParameterLocation.Header,
            Name = "Authorization?",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            },

            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri( settings.AuthorizationUrl ), 
                    TokenUrl = new Uri( settings.TokenUrl), 
                    Scopes = settings.Scopes
                }
            }
        };

        services.AddSwaggerGen(config =>
        {
            config.AddSecurityDefinition(scheme.Reference.Id, scheme);
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    { scheme, Array.Empty<string>()}
            });
        });


        return services;
    }
}
/*

                      
*/