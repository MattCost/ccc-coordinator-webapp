using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.ODataErrors;

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
    public async Task<ActionResult<RideEvent>> Create([FromBody] RideEventCreateModel createModel)
    {
        Logger.LogDebug("Entering Create Ride Event. Generating model");
        var model = new RideEvent
        {
            Id = Guid.NewGuid(),
            Name = createModel.Name,
            Description = createModel.Description,
            StartTime = createModel.StartTime,
            Location = createModel.Location,
            RideIds = createModel.Rides,
            EventType = createModel.EventType
        };

        if(createModel.Facilitator)
        {
            model.SupportPersonnel[CoordinatorRole.Facilitator] = new CoordinatorEntry{ RequiredCount = 1 };
        }
        
        if(createModel.GrillMaster)
        {
            model.SupportPersonnel[CoordinatorRole.GrillMaster] = new CoordinatorEntry{ RequiredCount = 1 };
        }

        Logger.LogDebug("Generated Model {Json}", System.Text.Json.JsonSerializer.Serialize(model));
        return await EntityProviderActionHelper( async () => {await EntityProvider.UpdateRideEvent(model); return model;}, "Unable to create Ride Event");
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
            model.EventType = updateModel.EventType ?? model.EventType;
            
            if(updateModel.Facilitator.HasValue)
            {
                if(updateModel.Facilitator.Value && !model.SupportPersonnel.ContainsKey(CoordinatorRole.Facilitator) )
                {
                    model.SupportPersonnel[CoordinatorRole.Facilitator] = new CoordinatorEntry{RequiredCount = 1};
                }

                if( !updateModel.Facilitator.Value && model.SupportPersonnel.ContainsKey(CoordinatorRole.Facilitator) )
                {
                    model.SupportPersonnel.Remove(CoordinatorRole.Facilitator);
                }
            }

            if(updateModel.GrillMaster.HasValue)
            {
                if(updateModel.GrillMaster.Value && !model.SupportPersonnel.ContainsKey(CoordinatorRole.GrillMaster) )
                {
                    model.SupportPersonnel[CoordinatorRole.GrillMaster] = new CoordinatorEntry{RequiredCount = 1};
                }

                if( !updateModel.GrillMaster.Value && model.SupportPersonnel.ContainsKey(CoordinatorRole.GrillMaster) )
                {
                    model.SupportPersonnel.Remove(CoordinatorRole.GrillMaster);
                }
            }

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
    public async Task<ActionResult> Delete([FromRoute] Guid id, [FromQuery] bool force = false)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteRideEvent(id, force), "Unable to delete ride event");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.RestoreRideEvent(id), "Unable to restore ride event");
    }    

    [HttpGet("{id:guid}/signups")]
    public async Task<ActionResult> GetAllSignups([FromRoute] Guid id)
    {
        var output = new List<SignupEntry>();
        var model = await EntityProvider.GetRideEvent(id);

        foreach (var rideId in model.RideIds)
        {
            var ride = await EntityProvider.GetGroupRide(rideId);
            foreach (var role in ride.Coordinators.Keys)
            {
                var current = ride.Coordinators[role].CoordinatorIds.Select(coordinatorId => new SignupEntry { CoordinatorRole = role, EntityType=Enums.EntityTypes.GroupRide ,EntityId = ride.Id, UserId = coordinatorId });
                output.AddRange(current);

                var blanksRequired = ride.Coordinators[role].RequiredCount - current.Count();
                for (int i = 0; i < blanksRequired; i++)
                {
                    output.Add(new SignupEntry { CoordinatorRole = role, EntityType=Enums.EntityTypes.GroupRide ,EntityId = ride.Id, UserId = string.Empty });
                }
            }
        }
        
        foreach(var supportRole in model.SupportPersonnel.Keys)
        {
            var current = model.SupportPersonnel[supportRole].CoordinatorIds.Select(coordinatorId => new SignupEntry { CoordinatorRole = supportRole, EntityType=Enums.EntityTypes.RideEvent, EntityId = model.Id, UserId = coordinatorId });
            output.AddRange(current);

            var blanksRequired = model.SupportPersonnel[supportRole].RequiredCount - current.Count();
            for (int i = 0; i < blanksRequired; i++)
            {
                output.Add(new SignupEntry { CoordinatorRole = supportRole, EntityType = Enums.EntityTypes.RideEvent, EntityId = model.Id, UserId = string.Empty });
            }   
        }
        return Ok(output);
    }

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpPatch("{id:guid}/support/{role:coordinatorRole}")]
    public async Task<ActionResult> Signup([FromRoute] Guid id, [FromRoute] CoordinatorRole role, [FromBody] string coordinatorId)
    {
        //get group ride, get parent event, get all rides, if user is already signed up bad request
        return await EntityProviderActionHelper(async () =>
        {
            var model = await EntityProvider.GetRideEvent(id);
            if (!model.SupportPersonnel.ContainsKey(role) )
            {
                throw new InvalidOperationException($"Support Role {role} not required");
            }
            if (model.SupportPersonnel[role].RequiredCountMet)
            {
                throw new InvalidOperationException($"Support Role {role} already filled");
            }            
            model.SupportPersonnel[role].CoordinatorIds.Add(coordinatorId);
            model.UnavailableCoordinators.Remove(coordinatorId);
            await EntityProvider.UpdateRideEvent(model);
        }, "Unable to signup");
    }

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpDelete("{id:guid}/support/{role:coordinatorRole}")]
    public async Task<ActionResult> DeleteSignup([FromRoute] Guid id, [FromRoute] CoordinatorRole role, [FromBody] string coordinatorId)
    {
        return await EntityProviderActionHelper(async () =>
        {
            var model = await EntityProvider.GetRideEvent(id);
            if(!model.SupportPersonnel.ContainsKey(role))
            {
                throw new InvalidOperationException($"Support Role {role} not required. Unable to delete");
            }

            if (!model.SupportPersonnel[role].CoordinatorIds.Contains(coordinatorId))
            {
                throw new InvalidOperationException("Coordinator is not signed up. Unable to delete");
            }
            model.SupportPersonnel[role].CoordinatorIds.Remove(coordinatorId);
            await EntityProvider.UpdateRideEvent(model);
        }, "Unable to remove signup");
    }    

    [Authorize(Policy =CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpPost("{id:guid}/unavailable/{coordinatorId}")]
    public async Task<ActionResult> AddUnavailable([FromRoute] Guid id, [FromRoute] string coordinatorId)
    {
        return await EntityProviderActionHelper(async() =>
        {
            var model = await EntityProvider.GetRideEvent(id);
            if(!model.UnavailableCoordinators.Contains(coordinatorId))
            {
                model.UnavailableCoordinators.Add(coordinatorId);
                await EntityProvider.UpdateRideEvent(model);
            }
            
            //check support
            var supp = model.SupportPersonnel.Where( entry => entry.Value.CoordinatorIds.Contains(coordinatorId));
            foreach(var entry in supp)
            {
                var role = entry.Key;
                model.SupportPersonnel[role].CoordinatorIds.Remove(coordinatorId);
                await EntityProvider.UpdateRideEvent(model);
            }

            //check all child rides
            foreach(var rideId in model.RideIds)
            {
                var rideModel = await EntityProvider.GetGroupRide(rideId);
                var entries = rideModel.Coordinators.Where( entry => entry.Value.CoordinatorIds.Contains(coordinatorId));
                if(entries.Any()) 
                {
                    foreach(var entry in entries)
                    {
                        rideModel.Coordinators[entry.Key].CoordinatorIds.Remove(coordinatorId);
                    }
                    await EntityProvider.UpdateGroupRide(rideModel);
                }
            }

        }, "unable to set as unavailable");
    }

    [Authorize(Policy =CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpDelete("{id:guid}/unavailable/{coordinatorId}")]
    public async Task<ActionResult> RemoveUnavailable([FromRoute] Guid id, [FromRoute] string coordinatorId)
    {
        return await EntityProviderActionHelper(async() =>
        {
            var model = await EntityProvider.GetRideEvent(id);
            if(model.UnavailableCoordinators.Remove(coordinatorId))
            {
                await EntityProvider.UpdateRideEvent(model);
            }
        }, "unable to remove from unavailable");
    }
    
}


