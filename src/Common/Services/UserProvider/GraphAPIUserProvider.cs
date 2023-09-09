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

    public async Task<IEnumerable<User>> GetCoordinators()
    {
        var users = await _graphServiceClient.Users.GetAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = new string[] { "displayName", "id", Common.Authorization.Enums.IsCoordinatorAttribute, Common.Authorization.Enums.IsCoordinatorAdminAttribute };
            requestConfig.QueryParameters.Filter = $"{Common.Authorization.Enums.IsCoordinatorAttribute} eq true";
        });
        if (users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select(user => new User { DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me", AdditionalData = user.AdditionalData});            
    }

public async Task<IEnumerable<User>> GetCoordinatorAdmins()
    {
        var users = await _graphServiceClient.Users.GetAsync((requestConfig) =>
        {
            requestConfig.QueryParameters.Select = new string[] { "displayName", "id", Common.Authorization.Enums.IsCoordinatorAttribute, Common.Authorization.Enums.IsCoordinatorAdminAttribute };
            requestConfig.QueryParameters.Filter = $"{Common.Authorization.Enums.IsCoordinatorAdminAttribute} eq true";
        });
        if (users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select(user => new User { DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me", AdditionalData = user.AdditionalData});
    }

    public async Task AssignCoordinator(string userId)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();
        if (user is null) throw new EntityNotFoundException($"UserId {userId} not found");
        user.AdditionalData[Common.Authorization.Enums.IsCoordinatorAttribute] = true;
        await _graphServiceClient.Users[userId].PatchAsync(user);
    }

    public async Task RemoveCoordinator(string userId)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();
        if (user is null) throw new EntityNotFoundException($"UserId {userId} not found");
        user.AdditionalData[Common.Authorization.Enums.IsCoordinatorAttribute] = false;
        await _graphServiceClient.Users[userId].PatchAsync(user);
    }

    public async Task AssignCoordinatorAdmin(string userId)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();
        if (user is null) throw new EntityNotFoundException($"UserId {userId} not found");
        user.AdditionalData[Common.Authorization.Enums.IsCoordinatorAdminAttribute] = true;
        await _graphServiceClient.Users[userId].PatchAsync(user);
    }

    public async Task RemoveCoordinatorAdmin(string userId)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();
        if (user is null) throw new EntityNotFoundException($"UserId {userId} not found");
        user.AdditionalData[Common.Authorization.Enums.IsCoordinatorAdminAttribute] = false;
        await _graphServiceClient.Users[userId].PatchAsync(user);
    }

}