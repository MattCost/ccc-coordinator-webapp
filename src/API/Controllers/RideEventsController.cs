using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

public class RideEventsController : EntityProviderBaseController
{
    public RideEventsController(ILogger<RideEventsController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet()]
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

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] RideEventCreateModel createModel)
    {
        Logger.LogDebug("Entering Create Ride Event. Generating model");
        var model = new RideEvent
        {
            Id = Guid.NewGuid(),
            Name = createModel.Name,
            Description = createModel.Description,
            StartTime = createModel.StartTime,
            Location = createModel.Location,
            RideIds = createModel.Rides
        };
        Logger.LogDebug("Generated Model {Json}", System.Text.Json.JsonSerializer.Serialize(model));
        return await EntityProviderActionHelper( async () => await EntityProvider.UpdateRideEvent(model), "Unable to create Ride Event");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] RideEventUpdateModel updateModel)
    {
        Logger.LogTrace("Entering Update for Id {Id} with model {Model}", id, updateModel);

        return await EntityProviderActionHelper( async () =>
        {
            var model = await EntityProvider.GetRideEvent(id);
            Logger.LogDebug("Model: {Model}", model); 
            model.Name = updateModel.Name ?? model.Name;
            model.Description = updateModel.Description ?? model.Description;
            model.StartTime = updateModel.StartTime ?? model.StartTime;
            model.Location = updateModel.Location ?? model.Location;
            Logger.LogDebug("Model: {Model}", model);
            await EntityProvider.UpdateRideEvent(model);
        },"Unable to update Ride Event");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpDelete("{id:guid}/groupRides/{rideId:guid}")]
    public async Task<ActionResult> RemoveRide([FromRoute] Guid id, [FromRoute] Guid rideId)
    {
        var model = await EntityProvider.GetRideEvent(id);
        model.RideIds.Remove(rideId);
        await EntityProvider.UpdateRideEvent(model);
        return Ok();
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]    
    [HttpPatch("{id:guid}/groupRides/{rideId:guid}")]
    public async Task<ActionResult> AddRide([FromRoute] Guid id, [FromRoute] Guid rideId)
    {
        var model = await EntityProvider.GetRideEvent(id);
        model.RideIds.Add(rideId);
        await EntityProvider.UpdateRideEvent(model);
        return Ok();
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteRideEvent(id), "Unable to delete ride event");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.RestoreRideEvent(id), "Unable to restore ride event");
    }    

}


