using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

[Authorize]
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

    // Full update / create with explicit id
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Put([FromRoute] Guid id, [FromBody] BikeRoute model)
    {
        if(model.Id != id)
        {
            return BadRequest("Not able to change model id.");
        }
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateBikeRoute(model), "Unable to update Bike Route");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] BikeRouteCreateModel createModel)
    {
        var model = new BikeRoute
        {
            Id = Guid.NewGuid(),
            Name = createModel.Name,
            Description = createModel.Description,
            Distance = createModel.Distance
        };
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateBikeRoute(model), "Unable to create Bike Route");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteBikeRoute(id), "Unable to delete bike route");
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.RestoreBikeRoute(id), "Unable to restore bike route");
    }

    [HttpPost("{id:guid}/cues")]
    public async Task<ActionResult> AddCue([FromRoute] Guid id, [FromBody] CueEntry cueEntry)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        model.Cues.Add(cueEntry);
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }

    [HttpDelete("{id:guid}/cues/{index:int}")]
    public async Task<ActionResult> DeleteCue([FromRoute] Guid id, [FromRoute] int index)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        if(index > model.Cues.Count)
        {
            return BadRequest("index is out of range");
        }
        model.Cues.RemoveAt(index);
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }

    [HttpPatch("{id:guid}/cues/{index:int}")]
    public async Task<ActionResult> InsertCue([FromRoute] Guid id, [FromRoute] int index, [FromBody] CueEntry cue)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        if(index > model.Cues.Count)
        {
            return BadRequest("index is out of range");
        }

        model.Cues.Insert(index, cue);
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }
}


