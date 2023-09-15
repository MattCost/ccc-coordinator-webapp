using CCC.Entities;
using CCC.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CCC.Services.UserProvider;

public class GraphAPIUserProvider : IUserProvider
{
    private readonly GraphServiceClient _graphServiceClient;
    protected readonly ILogger<GraphAPIUserProvider> _logger;

    public GraphAPIUserProvider(ILogger<GraphAPIUserProvider> logger, GraphServiceClient graphServiceClient)
    {
        _logger = logger;
        _graphServiceClient = graphServiceClient;
    }

    private readonly string[] _userSelectParameters =  { "displayName", "id", "emails", CCC.Authorization.Enums.IsCoordinatorAttribute, CCC.Authorization.Enums.IsCoordinatorAdminAttribute, CCC.Authorization.Enums.IsContributorAttribute, CCC.Authorization.Enums.IsAdminAttribute };

    public async Task<IEnumerable<User>> GetCoordinators()
    {
        var users = await _graphServiceClient.Users.GetAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = _userSelectParameters;  
            requestConfig.QueryParameters.Filter = $"{CCC.Authorization.Enums.IsCoordinatorAttribute} eq true";
        });
        if (users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select(user => new User { DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me", AdditionalData = user.AdditionalData });
    }

    public async Task<IEnumerable<User>> GetCoordinatorAdmins()
    {
        var users = await _graphServiceClient.Users.GetAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = _userSelectParameters;  
            requestConfig.QueryParameters.Filter = $"{CCC.Authorization.Enums.IsCoordinatorAdminAttribute} eq true";
        });
        if (users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select(user => new User { DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me", AdditionalData = user.AdditionalData });
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        var users = await _graphServiceClient.Users.GetAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = _userSelectParameters;  
            // requestConfig.QueryParameters.Filter = $"{CCC.Authorization.Enums.IsCoordinatorAdminAttribute} eq true";
        });
        if (users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select(user => new User { DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me", AdditionalData = user.AdditionalData });
    }    

    private async Task SetAdditionalDataAttribute(string userId, string attribute, bool value)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();
        if (user is null) throw new UserNotFoundException($"UserId {userId} not found");
        user.AdditionalData[attribute] = value;
        await _graphServiceClient.Users[userId].PatchAsync(user);
    }

    public async Task AssignCoordinator(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsCoordinatorAttribute, true);
    }

    public async Task RemoveCoordinator(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsCoordinatorAttribute, false);
    }

    public async Task AssignCoordinatorAdmin(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsCoordinatorAdminAttribute, true);
    }

    public async Task RemoveCoordinatorAdmin(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsCoordinatorAdminAttribute, false);
    }

    public async Task AssignContributor(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsContributorAttribute, true);
    }

    public async Task RemoveContributor(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsContributorAttribute, false);
    }

    public async Task AssignAdmin(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsAdminAttribute, true);
    }

    public async Task RemoveAdmin(string userId)
    {
        await SetAdditionalDataAttribute(userId, CCC.Authorization.Enums.IsAdminAttribute, false);
    }
}