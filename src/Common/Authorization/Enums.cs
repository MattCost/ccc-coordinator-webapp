//todo rename to ccc.auth
namespace CCC.Authorization;

public static class Enums
{
    /// <summary>
    /// The Extension App Id from Azure B2C, with underscores removed
    /// </summary>
    private const string _appId = "eea4730478fb41f1941cd85c3fb8ba03";

    public const string ReadOnlyPolicy = nameof(ReadOnlyPolicy);
    public const string CoordinatorAdminPolicy = nameof(CoordinatorAdminPolicy);
    public const string CoordinatorPolicy = nameof(CoordinatorPolicy);
    public const string ContributorPolicy = nameof(ContributorPolicy);
    public const string AdminPolicy = nameof(AdminPolicy);
    
    // The token from the b2c appreg, returns claims without the guid.
    public const string IsCoordinatorClaim = $"extension_IsCoordinator";
    public const string IsCoordinatorAdminClaim = $"extension_IsCoordinatorAdmin";
    public const string IsContributorClaim = $"extension_IsContributor";
    public const string IsAdminClaim = $"extension_IsAdmin";

    // But to assign the attribute to a user, you must include the guid (minus underscores) in the name.
    public const string IsCoordinatorAttribute = $"extension_{_appId}_IsCoordinator";
    public const string IsCoordinatorAdminAttribute = $"extension_{_appId}_IsCoordinatorAdmin";
    public const string IsContributorAttribute = $"extension_{_appId}_IsContributor";
    public const string IsAdminAttribute = $"extension_{_appId}_IsAdmin";

    public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
}