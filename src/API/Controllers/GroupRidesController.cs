using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupRidesController : EntityProviderBaseController
{
    public GroupRidesController(ILogger<GroupRidesController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<GroupRide>>> Get()
    {
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetAllGroupRides();}, "Unable to get all groupRides");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupRide>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetGroupRide(id);}, "Unable to get groupRide");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] GroupRide model)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateGroupRide(model), "Unable to update groupRide");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteGroupRide(id), "Unable to delete groupRide");
    }
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.RestoreGroupRide(id), "Unable to restore groupRide");
    }
}


