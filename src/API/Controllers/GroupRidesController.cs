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

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] GroupRideCreateModel createModel)
    {
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
        return await EntityProviderActionHelper(async () => await EntityProvider.UpdateGroupRide(model), "Unable to create groupRide");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper(async () => await EntityProvider.DeleteGroupRide(id), "Unable to delete groupRide");
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult> Restore([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper(async () => await EntityProvider.RestoreGroupRide(id), "Unable to restore groupRide");
    }

    [HttpPatch("{id:guid}/coordinators/{role:coordinatorRole}")] // signup. need userId, position
    public async Task<ActionResult> Signup([FromRoute] Guid id, [FromRoute] CoordinatorRole role, [FromBody] string coordinatorId)
    {
        return await EntityProviderActionHelper(async () =>
        {
            var model = await EntityProvider.GetGroupRide(id);
            if (model.Coordinators[role].RequiredCountMet)
            {
                throw new InvalidOperationException("Ride is already full");
            }
            model.Coordinators[role].CoordinatorIds.Add(coordinatorId);
            await EntityProvider.UpdateGroupRide(model);
        }, "Unable to signup");
    }

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



