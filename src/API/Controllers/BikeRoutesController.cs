using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

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

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] BikeRouteUpdateModel updateModel)
    {
        Logger.LogTrace("Entering Update for Id {Id} with model {Model}", id, updateModel);

        return await EntityProviderActionHelper( async () =>
        {
            var model = await EntityProvider.GetBikeRoute(id);
            Logger.LogDebug("Model: {Model}", model); 
            model.Name = updateModel.Name ?? model.Name;
            model.Distance = updateModel.Distance ?? model.Distance;
            model.Description = updateModel.Description ?? model.Description;
            model.RideWithGPSRouteId = updateModel.RideWithGPSRouteId ?? model.RideWithGPSRouteId;
            model.GarminConnectRouteId = updateModel.GarminConnectRouteId ?? model.GarminConnectRouteId;
            Logger.LogDebug("Model: {Model}", model);
            await EntityProvider.UpdateBikeRoute(model);
        },"Unable to update Bike Route");
    }
    
    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPost]
    public async Task<ActionResult<BikeRoute>> Create([FromBody] BikeRouteCreateModel createModel)
    {
        Logger.LogTrace("Entering Create. Model {Model}", createModel);        
        var model = new BikeRoute
        {
            Id = Guid.NewGuid(),
            Name = createModel.Name,
            Description = createModel.Description,
            Distance = createModel.Distance,
            RideWithGPSRouteId = createModel.RideWithGPSRouteId,
            GarminConnectRouteId = createModel.GarminConnectRouteId

        };  
        return await EntityProviderActionHelper( async () => { await EntityProvider.UpdateBikeRoute(model); return model;}, "Unable to create Bike Route");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        Logger.LogTrace("Entering Delete");
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteBikeRoute(id), "Unable to delete bike route");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        Logger.LogTrace("Entering Restore");
        return await EntityProviderActionHelper( async () => await EntityProvider.RestoreBikeRoute(id), "Unable to restore bike route");
    }

    /*
        Create (end)        POST id/cues - adds a single cue to the end
        Create (insert)     POST id/cues/index - adds a single cue at the specified index. 
        Delete              DELETE id/cues/index - deletes a single cue at the specified index
        Update              PUT id/cues/index - updates a single cue at the specified index
        Update              PUT id/cues - replaces all cues on model

    */

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPost("{id:guid}/cues")]
    public async Task<ActionResult> AddCue([FromRoute] Guid id, [FromBody] CueEntry cueEntry)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        model.Cues.Add(cueEntry);
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPost("{id:guid}/cues/{index:int}")]
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

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/cues")]
    public async Task<ActionResult> ReplaceAllCue([FromRoute] Guid id, [FromBody] List<CueEntry> cues)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        model.Cues = cues;
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }


    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpDelete("{id:guid}/cues")]
    public async Task<ActionResult> DeleteAllCue([FromRoute] Guid id)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        model.Cues = new();
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }


    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
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

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/cues/{index:int}")]
    public async Task<ActionResult> UpdateCue([FromRoute] Guid id, [FromRoute] int index, [FromBody] CueEntry cue)
    {
        var model = await EntityProvider.GetBikeRoute(id);
        if(index > model.Cues.Count)
        {
            return BadRequest("index is out of range");
        }
        model.Cues[index] = cue;
        await EntityProvider.UpdateBikeRoute(model);
        return Ok();
    }
}


