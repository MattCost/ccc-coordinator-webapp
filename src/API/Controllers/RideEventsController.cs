using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RideEventsController : EntityProviderBaseController
{
    public RideEventsController(ILogger<RideEventsController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<RideEvent>>> Get()
    {
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetAllRideEvents();}, "Unable to get all ride events");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RideEvent>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetRideEvent(id);}, "Unable to get Ride Event");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] RideEvent model)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateRideEvent(model), "Unable to update Ride Event");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteRideEvent(id), "Unable to delete ride event");
    }

}


