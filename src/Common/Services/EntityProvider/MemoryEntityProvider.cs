using CCC.Entities;
using CCC.Exceptions;

namespace CCC.Services.EntityProvider;

public class MemoryEntityProvider : IEntityProvider
{
    private Dictionary<Guid, BikeRoute> _bikeRoutes = new();
    private Dictionary<Guid, GroupRide> _groupRides = new();
    private Dictionary<Guid, RideEvent> _rideEvents = new();
    private Dictionary<Guid, Coordinator> _coordinators = new();
    public Task DeleteBikeRoute(Guid routeId)
    {      
        if(!_bikeRoutes.ContainsKey(routeId))  
            throw new EntityNotFoundException(typeof(BikeRoute), routeId);

        var rides = _groupRides.Where( ride => ride.Value.BikeRouteId == routeId);
        
        if(rides.Any())
            throw new EntityLockedException($"Bike Route {routeId} is in use. Can't be deleted");

        _bikeRoutes.Remove(routeId);
        
        return Task.CompletedTask;

    }

    public Task DeleteCoordinator(Guid coordinatorId)
    {
        _coordinators.Remove(coordinatorId);
        return Task.CompletedTask;
    }

    public Task DeleteGroupRide(Guid rideId)
    {
        if(!_groupRides.ContainsKey(rideId))
            throw new EntityNotFoundException(typeof(GroupRide), rideId);
        
        _groupRides.Remove(rideId);

        _rideEvents.Where( rideEvent => rideEvent.Value.Rides.Contains(rideId)).ToList().ForEach( rideEvent => rideEvent.Value.Rides.Remove(rideId));

        return Task.CompletedTask;
    }

    public Task DeleteRideEvent(Guid eventId)
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

    public Task<BikeRoute> GetBikeRoute(Guid routeId)
    {
        return _bikeRoutes.ContainsKey(routeId) ? Task.FromResult(_bikeRoutes[routeId]) : 
            throw new EntityNotFoundException(typeof(BikeRoute), routeId);
    }

    public Task<IEnumerable<Coordinator>> GetCoordinators()
    {
        return Task.FromResult<IEnumerable<Coordinator>>(_coordinators.Values.ToList());
    }

    public Task<GroupRide> GetGroupRide(Guid rideId)
    {
        return _groupRides.ContainsKey(rideId) ? Task.FromResult(_groupRides[rideId]) :
            throw new EntityNotFoundException(typeof(GroupRide), rideId);
    }

    public Task<RideEvent> GetRideEvent(Guid eventId)
    {
        return _rideEvents.ContainsKey(eventId) ? Task.FromResult(_rideEvents[eventId]) :
            throw new EntityNotFoundException(typeof(RideEvent), eventId);
    }

    public Task RestoreBikeRoute(Guid routeId)
    {
        throw new NotImplementedException();
    }

    public Task RestoreGroupRide(Guid rideId)
    {
        throw new NotImplementedException();
    }

    public Task RestoreRideEvent(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateBikeRoute(BikeRoute bikeRoute)
    {
        _bikeRoutes[bikeRoute.Id] = bikeRoute;
        return Task.CompletedTask;
    }

    public Task UpdateCoordinator(Coordinator coordinator)
    {
        _coordinators[coordinator.Id] = coordinator;
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