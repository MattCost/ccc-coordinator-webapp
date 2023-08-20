using CCC.Entities;
using CCC.Exceptions;

namespace CCC.Services.EntityProvider;

public class MemoryEntityProvider : IEntityProvider
{
    private Dictionary<int, BikeRoute> _bikeRoutes = new();
    private Dictionary<int, GroupRide> _groupRides = new();
    private Dictionary<int, RideEvent> _rideEvents = new();
    public Task DeleteBikeRoute(int routeId)
    {      
        if(!_bikeRoutes.ContainsKey(routeId))  
            throw new EntityNotFoundException(typeof(BikeRoute), routeId);

        var rides = _groupRides.Where( ride => ride.Value.BikeRouteId == routeId);
        
        if(rides.Any())
            throw new EntityLockedException($"Bike Route {routeId} is in use. Can't be deleted");

        _bikeRoutes.Remove(routeId);
        
        return Task.CompletedTask;

    }

    public Task DeleteGroupRide(int rideId)
    {
        if(!_groupRides.ContainsKey(rideId))
            throw new EntityNotFoundException(typeof(GroupRide), rideId);
        
        _groupRides.Remove(rideId);

        _rideEvents.Where( rideEvent => rideEvent.Value.Rides.Contains(rideId)).ToList().ForEach( rideEvent => rideEvent.Value.Rides.Remove(rideId));

        return Task.CompletedTask;
    }

    public Task DeleteRideEvent(int eventId)
    {
        if(!_rideEvents.ContainsKey(eventId))
            throw new EntityNotFoundException(typeof(RideEvent), eventId);
        
        if(_rideEvents[eventId].Rides.Any())
        {
            throw new EntityLockedException($"Ride Event {eventId} has rides listed. Delete rides first");
        }

        _rideEvents.Remove(eventId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<BikeRoute>> GetAllBikeRoutes()
    {
        return Task.FromResult<IEnumerable<BikeRoute>>(_bikeRoutes.Values.ToList());
    }

    public Task<IEnumerable<GroupRide>> GetAllGroupRides()
    {
        return Task.FromResult<IEnumerable<GroupRide>>(_groupRides.Values.ToList());
    }

    public Task<IEnumerable<RideEvent>> GetAllRideEvents()
    {
        return Task.FromResult<IEnumerable<RideEvent>>(_rideEvents.Values.ToList());
    }

    public Task<BikeRoute> GetBikeRoute(int routeId)
    {
        return _bikeRoutes.ContainsKey(routeId) ? Task.FromResult(_bikeRoutes[routeId]) : 
            throw new EntityNotFoundException(typeof(BikeRoute), routeId);
    }

    public Task<GroupRide> GetGroupRide(int rideId)
    {
        return _groupRides.ContainsKey(rideId) ? Task.FromResult(_groupRides[rideId]) :
            throw new EntityNotFoundException(typeof(GroupRide), rideId);
    }

    public Task<RideEvent> GetRideEvent(int eventId)
    {
        return _rideEvents.ContainsKey(eventId) ? Task.FromResult(_rideEvents[eventId]) :
            throw new EntityNotFoundException(typeof(RideEvent), eventId);
    }

    public Task UpdateBikeRoute(BikeRoute bikeRoute)
    {
        _bikeRoutes[bikeRoute.Id] = bikeRoute;
        return Task.CompletedTask;
    }

    public Task UpdateGroupRide(GroupRide groupRide)
    {
        _groupRides[groupRide.Id] = groupRide;
        return Task.CompletedTask;
    }

    public Task UpdateRideEvent(RideEvent rideEvent)
    {
        _rideEvents[rideEvent.Id] = rideEvent;
        return Task.CompletedTask;
    }
}