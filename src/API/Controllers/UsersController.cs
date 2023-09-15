using CCC.Entities;
using CCC.Exceptions;
using CCC.Services.EntityProvider;
using CCC.Services.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger, IUserProvider userProvider)
    {
        _logger = logger;
        _userProvider = userProvider;
    }

    //for now anyone can get coordinators, to fill out view models. todo - only coordinators can see coordinators?

    [HttpGet("coordinators")]
    public async Task<ActionResult<IEnumerable<User>>> GetCoordinators()
    {
        _logger.LogTrace("Entering GetCoordinators");
        try
        {
            return Ok(await _userProvider.GetCoordinators());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get all coordinators");
            return Problem(ex.Message);
        }
    }


    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpGet("coordinatorAdmins")]
    public async Task<ActionResult<IEnumerable<User>>> GetCoordinatorAdmins()
    {
        _logger.LogTrace("Entering GetCoordinatorAdmins");
        try
        {
            return Ok(await _userProvider.GetCoordinatorAdmins());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get all coordinatorAdmins");
            return Problem(ex.Message);
        }
    }

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        try
        {
            return Ok(await _userProvider.GetUsers());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get all users");
            return Problem(ex.Message);
        }
    }

    // I could turn all the following endpoints into just 2 endpoints AssignAttribute / RemoveAttribute, and take in a string/enum for the attribute name. 
    // But then I would need an auth handler to check the different auth requirements, or a more complex policy
    // Maybe if we add more roles (attributes) we will do that.

    private async Task<ActionResult> UserProviderActionHelper(Func<Task> action, string errorMessage)
    {
        try
        {
            await action();
            return Ok();
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found.");
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Exception. Error message: {Message}", errorMessage);
            return Problem(errorMessage);
        }
    }



    #region Coordinator

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpPatch("{userId}/coordinator")]
    public async Task<ActionResult> AssignCoordinator(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.AssignCoordinator(userId), "Unable to assign coordinator to user");
    }

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("{userId}/coordinator")]
    public async Task<ActionResult> RemoveCoordinator(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.RemoveCoordinator(userId), "Unable to remove coordinator from user");
    }
    #endregion

    #region CoordinatorAdmin

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/coordinatorAdmin")]
    public async Task<ActionResult> AssignCoordinatorAdmin(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.AssignCoordinatorAdmin(userId), "Unable to assign coordinator admin to user");
    }

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/coordinatorAdmin")]
    public async Task<ActionResult> RemoveCoordinatorAdmin(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.RemoveCoordinatorAdmin(userId), "Unable to remove coordinator admin from user");
    }
    #endregion

    #region Contributor

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/contributor")]
    public async Task<ActionResult> AssignContributor(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.AssignContributor(userId), "Unable to assign contributor to user");
    }

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/contributor")]
    public async Task<ActionResult> RemoveContributor(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.RemoveContributor(userId), "Unable to remove contributor from user");
    }
    #endregion

    #region Admin

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/admin")]
    public async Task<ActionResult> AssignAdmin(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.AssignAdmin(userId), "Unable to assign admin from user");
    }

    [Authorize(Policy = CCC.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/admin")]
    public async Task<ActionResult> RemoveAdmin(string userId)
    {
        return await UserProviderActionHelper(() => _userProvider.RemoveAdmin(userId), "Unable to remove admin from user");
    }

    #endregion

}