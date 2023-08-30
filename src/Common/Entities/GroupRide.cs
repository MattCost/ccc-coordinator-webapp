using System.Security;

namespace CCC.Entities;
public class CoordinatorEntry
{
    public List<string> CoordinatorIds { get; set;} = new();    //guid? email?
    public int RequiredCount { get; set; }
    public bool RequiredCountMet => CoordinatorIds.Count >= RequiredCount;
}
public class GroupRide
{
    public Guid Id { get; set; }
    public RideType RideType { get; set; }
    public Guid BikeRouteId { get; set; }
    public Guid RideEventId { get; set; }
    public Dictionary<CoordinatorRole, CoordinatorEntry> Coordinators {get;set;} = new();   
}