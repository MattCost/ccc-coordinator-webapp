using System.Runtime.CompilerServices;
using CCC.Common.ViewModels;
using CCC.Entities;
using CCC.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;

namespace CCC.API.Controllers;

public class FrontEndController : EntityProviderBaseController
{
    public FrontEndController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetRideEvent([FromRoute] Guid id)
    {
        var rideEvent = await EntityProvider.GetRideEvent(id);
        var viewModel = new RideEventViewModel
        {
            RideEvent = rideEvent
        };

        var tasks = viewModel.RideEvent.RideIds.Select( rideId => EntityProvider.GetGroupRide(rideId)).ToList();
        await Task.WhenAll(tasks);

        viewModel.GroupRides = tasks.Select( task => task.Result).ToList();

        var bikeRouteTasks = viewModel.GroupRides.Select( ride => EntityProvider.GetBikeRoute(ride.BikeRouteId)).ToList();
        await Task.WhenAll(bikeRouteTasks);

        viewModel.BikeRoutes = bikeRouteTasks.ToDictionary( task => task.Result.Id, task => task.Result);
        
        return Ok(viewModel);
    }
}
