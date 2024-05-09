using System.Text.RegularExpressions;
using CCC.Entities;

namespace CCC.Services.EntityProvider;


public interface IEntityProvider
{
    Task<BikeRoute> GetBikeRoute(Guid routeId);
    Task<IEnumerable<BikeRoute>> GetAllBikeRoutes();
    Task DeleteBikeRoute(Guid routeId);
    Task RestoreBikeRoute(Guid routeId);
    Task UpdateBikeRoute(BikeRoute bikeRoute);

    Task<GroupRide> GetGroupRide(Guid rideId);
    Task<IEnumerable<GroupRide>> GetAllGroupRides();
    Task DeleteGroupRide(Guid rideId);
    Task RestoreGroupRide(Guid rideId);
    Task UpdateGroupRide(GroupRide groupRide);

    Task<RideEvent> GetRideEvent(Guid eventId);
    Task<IEnumerable<RideEvent>> GetAllRideEvents();
    Task DeleteRideEvent (Guid eventId, bool force = false);
    Task RestoreRideEvent (Guid eventId);
    Task UpdateRideEvent(RideEvent rideEvent);

    Task<List<Guid>> GetFavoriteRoutes(string userId);
    Task AddFavoriteRoute(string userId, Guid bikeRouteId);
    Task RemoveFavoriteRoute(string userId, Guid bikeRouteId);

    Task<List<GroupRide>> GetRidesUsingRoute(Guid routeId);

}