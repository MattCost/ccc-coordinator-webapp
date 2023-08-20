namespace CCC.Entities;

public class GroupRide
{
    public int Id { get; set;}
    public RideType RideType { get; set; }
    public int BikeRouteId { get; set; }
    public int RideEventId { get; set; }
}