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

        viewModel.GroupRides = tasks.Select( task => task.Result).OrderBy(rideEvent => rideEvent.RideType).ToList();

        var bikeRouteTasks = viewModel.GroupRides.Select( ride => EntityProvider.GetBikeRoute(ride.BikeRouteId)).ToList();
        await Task.WhenAll(bikeRouteTasks);

        viewModel.BikeRoutes = bikeRouteTasks.Select( task => task.Result).ToList().DistinctBy( x => x.Id).ToDictionary( route => route.Id, route => new BikeRouteViewModel(route));
        
        var favorites = await EntityProvider.GetFavoriteRoutes(User.NameIdentifier());
        // favorites.ForEach( id => viewModel.BikeRoutes[id].IsFavorite = true);
        viewModel.BikeRoutes.Values.ToList().ForEach( bikeRoute => bikeRoute.IsFavorite = favorites.Contains(bikeRoute.Id));
        
        await allCoordinatorsTask;

        //Ids we need.
        var coordinatorIds = viewModel.GroupRides.SelectMany( ride => ride.Coordinators.Values.ToList()).SelectMany( entry => entry.CoordinatorIds).Distinct();
        var supportIds = viewModel.RideEvent.SupportPersonnel.SelectMany(entry => entry.Value.CoordinatorIds).Distinct();
        var allIds = coordinatorIds.Concat(supportIds).Distinct();
        viewModel.CoordinatorDisplayNames = allCoordinatorsTask.Result.Where( user => allIds.Contains(user.UserId) ).ToList().ToDictionary( user => user.UserId, user => user.DisplayName);
        return Ok(viewModel);
    }

    [HttpGet("BikeRoutes")]
    public async Task<ActionResult<List<BikeRouteViewModel>>> GetBikeRoutes()
    {
        var routes = EntityProvider.GetAllBikeRoutes();
        var favorites = EntityProvider.GetFavoriteRoutes(User.NameIdentifier());
        
        await Task.WhenAll(routes, favorites);

        var favoriteIds = favorites.Result ?? new List<Guid>();

        var output = routes.Result.Select( route => new BikeRouteViewModel(route) { IsFavorite = favoriteIds.Contains(route.Id)}).ToList();

        return output;
    }
}
