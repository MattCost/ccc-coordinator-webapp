using CCC.Entities;

namespace CCC.Common.ViewModels;

public class RideEventViewModel
{
    public RideEvent? RideEvent { get; set; }
    public List<GroupRide> GroupRides  { get; set; } = new();
    public Dictionary<Guid, BikeRoute> BikeRoutes { get; set; } = new();

}