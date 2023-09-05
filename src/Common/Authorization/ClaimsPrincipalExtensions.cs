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
}