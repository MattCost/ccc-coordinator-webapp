using CCC.Entities;

namespace CCC.Services.EntityProvider;


public interface IEntityProvider
{
    Task<BikeRoute> GetBikeRoute(int routeId);
    Task<IEnumerable<BikeRoute>> GetAllBikeRoutes();
    Task DeleteBikeRoute(int routeId);
    Task UpdateBikeRoute(BikeRoute bikeRoute);

    Task<GroupRide> GetGroupRide(int rideId);
    Task<IEnumerable<GroupRide>> GetAllGroupRides();
    Task DeleteGroupRide(int rideId);
    Task UpdateGroupRide(GroupRide groupRide);

    Task<RideEvent> GetRideEvent(int eventId);
    Task<IEnumerable<RideEvent>> GetAllRideEvents();
    Task DeleteRideEvent (int eventId);
    Task UpdateRideEvent(RideEvent rideEvent);


}