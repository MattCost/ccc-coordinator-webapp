using CCC.Entities;

namespace CCC.Services.UserProvider;

public interface IUserProvider
{
    Task<IEnumerable<User>> GetCoordinators();
    Task AssignCoordinator(string userId);
    Task RemoveCoordinator(string userId);

    Task<IEnumerable<User>> GetCoordinatorAdmins();
    Task AssignCoordinatorAdmin(string userId);
    Task RemoveCoordinatorAdmin(string userId);

}

