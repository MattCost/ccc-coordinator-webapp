using System.Security.Claims;

namespace CCC.Common.Authorization;

public static class AuthorizationExtensions
{
    public static bool IsCoordinator(this ClaimsPrincipal user)
    {
        return user.HasClaim( c=> c.Type == Enums.IsCoordinatorClaim && c.Value == "true");
    }

    public static bool IsCoordinatorAdmin(this ClaimsPrincipal user)
    {
        return user.HasClaim( c=> c.Type == Enums.IsCoordinatorAdminClaim && c.Value == "true");
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.HasClaim( c=> c.Type == Enums.IsAdminClaim && c.Value == "true");
    }

    public static bool IsContributor(this ClaimsPrincipal user)
    {
        return  user.HasClaim( c=> c.Type == Enums.IsContributorClaim && c.Value == "true");
    }

    public static string NameIdentifier(this ClaimsPrincipal user)
    {
        if(user.HasClaim( c=> c.Type == Enums.NameIdentifier) )
        {
            return user.Claims.Where( c => c.Type == Enums.NameIdentifier).First().Value;
        }
        return string.Empty;
    }
}