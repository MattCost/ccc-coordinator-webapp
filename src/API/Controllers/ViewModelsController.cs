using System.Runtime.CompilerServices;
using CCC.ViewModels;
using CCC.Entities;
using CCC.Services.EntityProvider;
using CCC.Services.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using CCC.Authorization;

namespace CCC.API.Controllers;

public class ViewModelsController : EntityProviderBaseController
{
    private readonly IUserProvider _userProvider;
    public ViewModelsController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider, IUserProvider userProvider) : base(logger, entityProvider)
    {
        _userProvider = userProvider;
    }

    [HttpGet("RideEvents/{id:guid}")]
    public async Task<ActionResult> GetRideEvent([FromRoute] Guid id)
    {
        var rideEvent = await EntityProvider.GetRideEvent(id);
        var viewModel = new RideEventViewModel
        {
            RideEvent = rideEvent
        };
        var allCoordinatorsTask = _userProvider.GetCoordinators();
        
        var tasks = viewModel.RideEvent.RideIds.Select( rideId => EntityProvider.GetGroupRide(rideId)).ToList();
        await Task.WhenAll(tasks);

        viewModel.GroupRides = tasks.Select( task => task.Result).ToList();

        var bikeRouteTasks = viewModel.GroupRides.Select( ride => EntityProvider.GetBikeRoute(ride.BikeRouteId)).ToList();
        await Task.WhenAll(bikeRouteTasks);

        viewModel.BikeRoutes = bikeRouteTasks.Select( task => task.Result).ToList().DistinctBy( x => x.Id).ToDictionary( route => route.Id, route => new BikeRouteViewModel(route));
        
        var favorites = await EntityProvider.GetFavoriteRoutes(User.NameIdentifier());
        favorites.ForEach( id => viewModel.BikeRoutes[id].IsFavorite = true);
        
        await allCoordinatorsTask;

        //Ids we need.
        var allIds = viewModel.GroupRides.SelectMany( ride => ride.Coordinators.Values.ToList()).SelectMany( entry => entry.CoordinatorIds).Distinct();
        viewModel.CoordinatorDisplayNames = allCoordinatorsTask.Result.Where( user => allIds.Contains(user.UserId) ).ToList().ToDictionary( user => user.UserId, user => user.DisplayName);
        return Ok(viewModel);
    }
}
