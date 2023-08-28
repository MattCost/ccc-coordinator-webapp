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
    Task DeleteRideEvent (Guid eventId);
    Task RestoreRideEvent (Guid eventId);
    Task UpdateRideEvent(RideEvent rideEvent);

    Task<IEnumerable<Coordinator>> GetCoordinators();
    Task DeleteCoordinator(Guid coordinatorId);
    Task UpdateCoordinator(Coordinator coordinator);

}