namespace CCC.Entities;

public class GroupRide
{
    public Guid Id { get; set;} = Guid.NewGuid();
    public RideType RideType { get; set; }
    public Guid BikeRouteId { get; set; }
    public Guid RideEventId { get; set; }
}