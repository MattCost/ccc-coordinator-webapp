using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;

namespace CCC.website;

public class RejectStaleSessionCookie : CookieAuthenticationEvents
{
    private readonly string[] _apiScopes = new string[] { "https://cccwebapp.onmicrosoft.com/ccc-webapp-api/API.Access" };

    public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        try
        {
            var tokenAcq = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
            var token = await tokenAcq.GetAccessTokenForUserAsync(
                scopes: _apiScopes,
                user: context.Principal
            );
        }
        catch (MicrosoftIdentityWebChallengeUserException ex) when (UserNotInCache(ex))
        {
            context.RejectPrincipal();
        }

    }

    private bool UserNotInCache(MicrosoftIdentityWebChallengeUserException ex)
    {
        var innerEx = ex.InnerException as MsalUiRequiredException;
        return innerEx != null && innerEx.ErrorCode == "user_null";
    }
}