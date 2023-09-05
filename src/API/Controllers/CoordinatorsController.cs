using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

public class CoordinatorsController : EntityProviderBaseController
{
    public CoordinatorsController(ILogger<CoordinatorsController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Coordinator>>> GetAll()
    {
        Logger.LogTrace("Entering GetAll");
        return await EntityProviderActionHelper( EntityProvider.GetCoordinators, "Unable to get all coordinators");
    }

    [HttpGet("admins")]
    public async Task<ActionResult<IEnumerable<Coordinator>>> GetAllAdmins()
    {
        Logger.LogTrace("Entering GetAll");
        return await EntityProviderActionHelper( EntityProvider.GetCoordinatorAdmins, "Unable to get all coordinators");
    }    

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpPatch("{userId}")]
    public async Task<ActionResult> Assign(string userId)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinator(userId), "Unable to assign coordinator to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
   
    [HttpPatch("{userId}/admin")]
    public async Task<ActionResult> AssignAdmin(string userId)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to assign coordinator admin to user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("{userId}")]
    public async Task<ActionResult> Remove(string userId)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.RemoveCoordinator(userId), "Unable to remove coordinator from user");
    }

    [Authorize(Policy = Common.Authorization.Enums.CoordinatorAdminPolicy)]
    [HttpDelete("{userId}/admin")]
    public async Task<ActionResult> RemoveAdmin(string userId)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.AssignCoordinatorAdmin(userId), "Unable to remove coordinator admin from user");
    }    

}
