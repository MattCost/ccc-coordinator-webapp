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


    [HttpGet("coordinators")]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        _logger.LogTrace("Entering GetAll");
        return Ok(await _userProvider.GetCoordinators());
        // return await EntityProviderActionHelper( EntityProvider.GetCoordinators, "Unable to get all coordinators");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpGet("coordinators/admins")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllAdmins()
    {
        _logger.LogTrace("Entering GetAll");
        return Ok(await _userProvider.GetCoordinatorAdmins());

        // return await EntityProviderActionHelper( EntityProvider.GetCoordinatorAdmins, "Unable to get all coordinators");
    }    

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpPatch("coordinators/{userId}")]
    public async Task<ActionResult> Assign(string userId)
    {
        await _userProvider.AssignCoordinator(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinator(userId), "Unable to assign coordinator to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
   
    [HttpPatch("coordinators/admin/{userId}")]
    public async Task<ActionResult> AssignAdmin(string userId)
    {
        await _userProvider.AssignCoordinatorAdmin(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to assign coordinator admin to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("coordinators/{userId}")]
    public async Task<ActionResult> Remove(string userId)
    {
        await _userProvider.RemoveCoordinator(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.RemoveCoordinator(userId), "Unable to remove coordinator from user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("coordinators/admin/{userId}")]
    public async Task<ActionResult> RemoveAdmin(string userId)
    {
        await _userProvider.RemoveCoordinatorAdmin(userId);
        return Ok();
        // return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to remove coordinator admin from user");
    }    

}
