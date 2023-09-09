using CCC.Entities;
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
        _logger.LogTrace("Entering GetAll");
        return Ok(await _userProvider.GetCoordinators());
        // return await EntityProviderActionHelper( EntityProvider.GetCoordinators, "Unable to get all coordinators");
    }

    // I could turn this into 2 endpoints AssignAttribute / RemoveAttribute, and take in a string/enum for the attribute name. 
    // But then I would need an auth handler to check the different auth requirements. 
    // Maybe if we add more roles (attributes) we will do that.

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpGet("coordinatorAdmins")]
    public async Task<ActionResult<IEnumerable<User>>> GetCoordinatorAdmins()
    {
        _logger.LogTrace("Entering GetAll");
        return Ok(await _userProvider.GetCoordinatorAdmins());
        // return await EntityProviderActionHelper( EntityProvider.GetCoordinatorAdmins, "Unable to get all coordinators");
    }

    #region Coordinator

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpPatch("{userId}/coordinator")]
    public async Task<ActionResult> AssignCoordinator(string userId)
    {
        await _userProvider.AssignCoordinator(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinator(userId), "Unable to assign coordinator to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("{userId}/coordinator")]
    public async Task<ActionResult> RemoveCoordinator(string userId)
    {
        await _userProvider.RemoveCoordinator(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.RemoveCoordinator(userId), "Unable to remove coordinator from user");
    }
    #endregion

    #region CoordinatorAdmin

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/coordinatorAdmin")]
    public async Task<ActionResult> AssignCoordinatorAdmin(string userId)
    {
        await _userProvider.AssignCoordinatorAdmin(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to assign coordinator admin to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/coordinatorAdmin")]
    public async Task<ActionResult> RemoveCoordinatorAdmin(string userId)
    {
        await _userProvider.RemoveCoordinatorAdmin(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to remove coordinator admin from user");
    }
    #endregion

    #region Contributor

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/contributor")]
    public async Task<ActionResult> AssignContributor(string userId)
    {
        await _userProvider.AssignContributor(userId);
        return Ok();
    }

    [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/contributor")]
    public async Task<ActionResult> RemoveContributor(string userId)
    {
        await _userProvider.RemoveContributor(userId);
        return Ok();
    }
    #endregion

    #region Admin

    // [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpPatch("{userId}/admin")]
    public async Task<ActionResult> AssignAdmin(string userId)
    {
        await _userProvider.AssignAdmin(userId);
        return Ok();
    }

    // [Authorize(Policy = Common.Authorization.Enums.AdminPolicy)]
    [HttpDelete("{userId}/admin")]
    public async Task<ActionResult> RemoveAdmin(string userId)
    {
        await _userProvider.RemoveAdmin(userId);
        return Ok();
    }


    #endregion

}