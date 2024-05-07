using CCC.Entities;

namespace CCC.ViewModels;

public class RideEventViewModel
{
    public RideEvent RideEvent { get; set; } = new();
    public List<GroupRide> GroupRides  { get; set; } = new();
    public Dictionary<Guid, BikeRouteViewModel> BikeRoutes { get; set; } = new();
    public Dictionary<string, string> CoordinatorDisplayNames {get;set;} = new();
    public List<string> AvailableCoordinators { get; set; } = new();
}