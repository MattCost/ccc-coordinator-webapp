using CCC.Authorization;
using CCC.Entities;
using CCC.ViewModels;
using CCC.website.Models;
using Microsoft.Identity.Abstractions;

namespace website.Pages;

public class IndexModel : PageModelBase
{
    public RideEventViewModel? NextRideEventVM {get;set;} = null;
    public IndexModel(ILogger<IndexModel> logger, IDownstreamApi api) : base(logger, api)
    {
    }

    public async Task OnGetAsync()
    {
        try
        {
            var result = await API.GetForUserAsync<List<RideEvent>>("API", options =>
            {
                options.RelativePath = "RideEvents";
            });
            Logger.LogDebug("Result from API {Result}", result);
            var allRides = result ?? new List<RideEvent>();
            ViewData["RideEvents"] = allRides;
            var nextRides = allRides.Where(ride => ride.StartTime >= DateTimeOffset.UtcNow ).OrderBy(ride => ride.StartTime); 
            if(nextRides.Any())
            {
                var nextRideEventId = nextRides.First().Id;
                var rideEventResult = await API.GetForUserAsync<RideEventViewModel>("API", options =>
                {       
                    options.RelativePath = $"ViewModels/RideEvents/{nextRideEventId}";
                });
                Logger.LogDebug("Result from API {Result}", rideEventResult);
                NextRideEventVM = rideEventResult;
                if(NextRideEventVM != null)
                {
                    var coordinatorSignedUpRide = NextRideEventVM.GroupRides.Where(ride => ride.Coordinators.Values.SelectMany( x => x.CoordinatorIds).Contains(User.NameIdentifier()));
                    var supportSignedUpRole = NextRideEventVM.RideEvent.SupportPersonnel.Where(kvp => kvp.Value.CoordinatorIds.Contains(User.NameIdentifier()));
                    if(coordinatorSignedUpRide.Any())
                    {
                        Logger.LogDebug("User is signed up for a role");
                        var rideType = coordinatorSignedUpRide.First().RideType;
                        var position = coordinatorSignedUpRide.First().Coordinators.Where(kvp => kvp.Value.CoordinatorIds.Contains(User.NameIdentifier())).First().Key;
                        var routeId = coordinatorSignedUpRide.First().BikeRouteId;
                        ViewData["RideType"] = rideType;
                        ViewData["Position"] = position;
                        ViewData["RouteId"] = routeId;
                        ViewData["RouteName"] = NextRideEventVM.BikeRoutes[routeId].Name;
                    }
                    

                    if(supportSignedUpRole.Any())
                    {
                        Logger.LogDebug("user is signed up for support");
                        ViewData["SupportRole"] = supportSignedUpRole.First().Key;
                    }

                    if(!coordinatorSignedUpRide.Any() && !supportSignedUpRole.Any())
                    {
                        ViewData["Message"] = "Go Signup!";
                    }


                    // var coordinatorSignedUcp = NextRideEventVM.GroupRides.Select(ride => ride.Coordinators.Values.ToList()).SelectMany(x => x).Where(entry => entry.CoordinatorIds.Contains(User.NameIdentifier()));
                    // var coordinatorSignedUp = NextRideEventVM.GroupRides.Select(ride => ride.Coordinators.Values.ToList()).SelectMany(x => x).Where(entry => entry.CoordinatorIds.Contains(User.NameIdentifier())).Any();
                    // var supportSignedUp = NextRideEventVM.RideEvent.SupportPersonnel.Select( entry => entry.Value).ToList().Where(entry => entry.CoordinatorIds.Contains(User.NameIdentifier())).Any();
                    // Logger.LogDebug("CoordinatorSignedUp: {coordinatorSignedUp}", coordinatorSignedUp);
                    // var IsSignedUp = coordinatorSignedUp | supportSignedUp;
                }


            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception trying to get RideEvents!");
            CurrentPageAction = "OnGetAsync";
            CurrentPageErrorMessage = ex.Message;
        }

    }
}
