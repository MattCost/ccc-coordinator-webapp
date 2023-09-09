using CCC.Entities;

namespace CCC.Services.UserProvider;

public interface IUserProvider
{
    Task<IEnumerable<User>> GetUsers();
    Task<IEnumerable<User>> GetCoordinators();
    Task AssignCoordinator(string userId);
    Task RemoveCoordinator(string userId);

    Task<IEnumerable<User>> GetCoordinatorAdmins();
    Task AssignCoordinatorAdmin(string userId);
    Task RemoveCoordinatorAdmin(string userId);

    Task AssignContributor(string userId);
    Task RemoveContributor(string userId);

    Task AssignAdmin(string userId);
    Task RemoveAdmin(string userId);

}

