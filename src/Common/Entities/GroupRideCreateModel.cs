namespace CCC.Entities;

public class GroupRideCreateModel
{
    public RideType RideType { get; set; }
    public Guid BikeRouteId { get; set; }
    public Guid RideEventId { get; set; }
}