namespace CCC.Common.Authorization;

public static class Enums
{
    /// <summary>
    /// The Extension App Id from Azure B2C, with underscores removed
    /// </summary>
    private const string _appId = "eea4730478fb41f1941cd85c3fb8ba03";

    public const string CoordinatorAdminPolicy = nameof(CoordinatorAdminPolicy);
    public const string CoordinatorPolicy = nameof(CoordinatorPolicy);
    
    public const string IsCoordinatorClaim = $"extension_IsCoordinator";
    public const string IsCoordinatorAdminClaim = $"extension_IsCoordinatorAdmin";

    public const string IsCoordinatorAttribute = $"extension_{_appId}_IsCoordinator";
    public const string IsCoordinatorAdminAttribute = $"extension_{_appId}_IsCoordinatorAdmin";

}