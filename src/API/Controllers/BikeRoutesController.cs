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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BikeRoute>>> GetAll()
    {
        Logger.LogTrace("Entering GetAll");
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetAllBikeRoutes();}, "Unable to get all bike routes");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BikeRoute>> GetById([FromRoute] Guid id)
    {
        Logger.LogTrace("Entering GetById");

        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetBikeRoute(id);}, "Unable to get Bike Route");
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] BikeRouteUpdateModel updateModel)
    {
        Logger.LogTrace("Entering Update for Id {Id} with model {Model}", id, updateModel);
        var modelResult = await EntityProviderActionHelper( async () => await EntityProvider.GetBikeRoute(id),"Unable to get Bike Route");
        
        if(modelResult.Result is not null)
            return modelResult.Result;

        if(modelResult.Value is null) 
            return Problem();

        
        var model = modelResult.Value;      
        Logger.LogDebug("Model: {Model}", model);        
        model.Name = updateModel.Name ?? model.Name;
        model.Distance = updateModel.Distance ?? model.Distance;
        model.Description = updateModel.Description ?? model.Description;
        Logger.LogDebug("Model: {Model}", model);        


        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateBikeRoute(model), "Unable to update Bike Route");
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] BikeRouteCreateModel createModel)
    {
        Logger.LogTrace("Entering Create. Model {Model}", createModel);        
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
        Logger.LogTrace("Entering Delete");
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteBikeRoute(id), "Unable to delete bike route");
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        Logger.LogTrace("Entering Restore");
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


