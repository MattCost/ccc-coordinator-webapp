using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class BikeRoutesController : EntityProviderBaseController
{
    public BikeRoutesController(ILogger<BikeRoutesController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<BikeRoute>>> Get()
    {
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetAllBikeRoutes();}, "Unable to get all bike routes");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BikeRoute>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetBikeRoute(id);}, "Unable to get Bike Route");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] BikeRoute model)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateBikeRoute(model), "Unable to update Bike Route");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteBikeRoute(id), "Unable to delete bike route");
    }

}


