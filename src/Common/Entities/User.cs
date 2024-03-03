namespace CCC.Entities;

public class User
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAdmin { get; init; } = false;
    public bool IsCoordinator { get; init; } = false;
    public bool IsCoordinatorAdmin { get; init; } = false;
    public bool IsContributor { get; init; } = false;

    // public IDictionary<string, object> AdditionalData {get;set;} = new Dictionary<string, object>();

    // public bool IsAdmin => AdditionalData.ContainsKey(CCC.Authorization.Enums.IsAdminAttribute) && ((System.Text.Json.JsonElement)AdditionalData[CCC.Authorization.Enums.IsAdminAttribute]).GetBoolean();
    // public bool IsCoordinator => AdditionalData.ContainsKey(CCC.Authorization.Enums.IsCoordinatorAttribute) && ((System.Text.Json.JsonElement)AdditionalData[CCC.Authorization.Enums.IsCoordinatorAttribute]).GetBoolean();
    // public bool IsCoordinatorAdmin => AdditionalData.ContainsKey(CCC.Authorization.Enums.IsCoordinatorAdminAttribute) &&((System.Text.Json.JsonElement)AdditionalData[CCC.Authorization.Enums.IsCoordinatorAdminAttribute]).GetBoolean();
    // public bool IsContributor => AdditionalData.ContainsKey(CCC.Authorization.Enums.IsContributorAttribute) &&((System.Text.Json.JsonElement)AdditionalData[CCC.Authorization.Enums.IsContributorAttribute]).GetBoolean();

    public User() { }

    // todo 

    public User(Microsoft.Graph.Models.User user)
    {
        UserId = user.Id ?? "???";
        DisplayName = user.DisplayName ?? "???";
    
        if (user.AdditionalData.ContainsKey(CCC.Authorization.Enums.IsAdminAttribute) && user.AdditionalData[CCC.Authorization.Enums.IsAdminAttribute] is bool)
        {
            IsAdmin = (bool)user.AdditionalData[CCC.Authorization.Enums.IsAdminAttribute];
        }

        if (user.AdditionalData.ContainsKey(CCC.Authorization.Enums.IsCoordinatorAttribute) && user.AdditionalData[CCC.Authorization.Enums.IsCoordinatorAttribute] is bool)
        {
            IsCoordinator = (bool)user.AdditionalData[CCC.Authorization.Enums.IsCoordinatorAttribute];
        }

        if (user.AdditionalData.ContainsKey(CCC.Authorization.Enums.IsCoordinatorAdminAttribute) && user.AdditionalData[CCC.Authorization.Enums.IsCoordinatorAdminAttribute] is bool)
        {
            IsCoordinatorAdmin = (bool)user.AdditionalData[CCC.Authorization.Enums.IsCoordinatorAdminAttribute];
        }

        if (user.AdditionalData.ContainsKey(CCC.Authorization.Enums.IsContributorAttribute) && user.AdditionalData[CCC.Authorization.Enums.IsContributorAttribute] is bool)
        {
            IsContributor = (bool)user.AdditionalData[CCC.Authorization.Enums.IsContributorAttribute];
        }

    }
}