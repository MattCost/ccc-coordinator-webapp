using CCC.Entities;
using CCC.Exceptions;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCC.API.Controllers;

public class GroupRidesController : EntityProviderBaseController
{
    public GroupRidesController(ILogger<GroupRidesController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<GroupRide>>> Get()
    {
        return await EntityProviderActionHelper(async () => { return await EntityProvider.GetAllGroupRides(); }, "Unable to get all groupRides");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupRide>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper(async () => { return await EntityProvider.GetGroupRide(id); }, "Unable to get groupRide");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPost]
    public async Task<ActionResult<GroupRide>> Create([FromBody] GroupRideCreateModel createModel)
    {
        try
        {
            await EntityProvider.GetBikeRoute(createModel.BikeRouteId);
        }
        catch(EntityNotFoundException)
        {
            return new BadRequestObjectResult(new {message = "Invalid Bike Route Id", createModel.BikeRouteId});
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Unable to verify Bike Route Id");
            return Problem(ex.Message);
        }

        try
        {
            await EntityProvider.GetRideEvent(createModel.RideEventId);
        }
        catch(EntityNotFoundException)
        {
            return new BadRequestObjectResult(new {message = "Invalid Event Id", createModel.RideEventId});
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Unable to verify Event Id");
            return Problem(ex.Message);
        }

        var model = new GroupRide
        {
            Id = Guid.NewGuid(),
            RideEventId = createModel.RideEventId,
            RideType = createModel.RideType,
            BikeRouteId = createModel.BikeRouteId,
        };

        switch (model.RideType)
        {
            case RideType.A30:
                model.Coordinators[CoordinatorRole.Lead] = new()
                {
                    RequiredCount = 2
                };
                model.Coordinators[CoordinatorRole.Sweep] = new()
                {
                    RequiredCount = 2
                };

                break;

            case RideType.B25:
            case RideType.B20:
            case RideType.B15:
                model.Coordinators[CoordinatorRole.Lead] = new()
                {
                    RequiredCount = 1
                };
                model.Coordinators[CoordinatorRole.Mid] = new()
                {
                    RequiredCount = 2
                };
                model.Coordinators[CoordinatorRole.Sweep] = new()
                {
                    RequiredCount = 1
                };
                break;

            case RideType.C:
                model.Coordinators[CoordinatorRole.Lead] = new()
                {
                    RequiredCount = 1
                };
                model.Coordinators[CoordinatorRole.Mid] = new()
                {
                    RequiredCount = 1
                };
                model.Coordinators[CoordinatorRole.Sweep] = new()
                {
                    RequiredCount = 1
                };
                break;
        }
        return await EntityProviderActionHelper(async () => {await EntityProvider.UpdateGroupRide(model); return model;}, "Unable to create groupRide");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPatch("{id:guid}/route/{bikeRouteId:guid}")]
    public async Task<ActionResult> UpdateRoute([FromRoute] Guid id, Guid bikeRouteId)
    {
        return await EntityProviderActionHelper( async () =>
        {
            var rideModel = await EntityProvider.GetGroupRide(id);
            Logger.LogDebug("Ride Model: {Model}", rideModel); 
            var routeModel = await EntityProvider.GetBikeRoute(bikeRouteId);
            Logger.LogDebug("Route Model: {Model}", routeModel);
            rideModel.BikeRouteId = bikeRouteId;
            Logger.LogDebug("Model: {Model}", rideModel);
            await EntityProvider.UpdateGroupRide(rideModel);
        },"Unable to update Group Ride");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper(async () => await EntityProvider.DeleteGroupRide(id), "Unable to delete groupRide");
    }

    [Authorize(Policy = CCC.Authorization.Enums.ContributorPolicy)]
    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper(async () => await EntityProvider.RestoreGroupRide(id), "Unable to restore groupRide");
    }

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpPatch("{id:guid}/coordinators/{role:coordinatorRole}")]
    public async Task<ActionResult> Signup([FromRoute] Guid id, [FromRoute] CoordinatorRole role, [FromBody] string coordinatorId)
    {
        //get group ride, get parent event, get all rides, if user is already signed up bad request
        return await EntityProviderActionHelper(async () =>
        {
            var model = await EntityProvider.GetGroupRide(id);
            var parentEvent = await EntityProvider.GetRideEvent( model.RideEventId);
            if (model.Coordinators[role].RequiredCountMet)
            {
                throw new InvalidOperationException("Ride is already full");
            }
            model.Coordinators[role].CoordinatorIds.Add(coordinatorId);
            await EntityProvider.UpdateGroupRide(model);
            
            if(parentEvent.UnavailableCoordinators.Remove(coordinatorId))
            {
                await EntityProvider.UpdateRideEvent(parentEvent);
            }
        }, "Unable to signup");
    }

    [Authorize(Policy = CCC.Authorization.Enums.CoordinatorPolicy)]
    [HttpDelete("{id:guid}/coordinators/{role:coordinatorRole}")]
    public async Task<ActionResult> DeleteSignup([FromRoute] Guid id, [FromRoute] CoordinatorRole role, [FromBody] string coordinatorId)
    {
        return await EntityProviderActionHelper(async () =>
        {
            var model = await EntityProvider.GetGroupRide(id);
            if (!model.Coordinators[role].CoordinatorIds.Contains(coordinatorId))
            {
                throw new InvalidOperationException("Coordinator is not signed up. Unable to delete");
            }
            model.Coordinators[role].CoordinatorIds.Remove(coordinatorId);
            await EntityProvider.UpdateGroupRide(model);
        }, "Unable to remove signup");
    }
}



