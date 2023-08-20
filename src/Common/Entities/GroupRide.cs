namespace CCC.Entities;

public class GroupRide
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RideType RideType { get; set; }
    public Guid BikeRouteId { get; set; }
    public Guid RideEventId { get; set; }
    public Dictionary<CoordinatorRole, List<Guid>> Coordinators { get; set; } = new Dictionary<CoordinatorRole, List<Guid>>
    {
        [CoordinatorRole.Lead] = new(),
        [CoordinatorRole.Mid] = new(),
        [CoordinatorRole.Sweep] = new()
    };

    public int LeadRequiredCount
    {
        get
        {
            return (RideType == RideType.A30) ? 2 : 1;
        }
    }
    public bool LeadCountMet { get { return Coordinators[CoordinatorRole.Lead].Count >= LeadRequiredCount; } }

    public int MidRequiredCount
    {
        get
        {
            switch (RideType)
            {
                case RideType.A30: return 0;
                case RideType.C: return 1;
                default: return 2;
            }
        }
    }
    public bool MidCountMet { get { return Coordinators[CoordinatorRole.Mid].Count >= MidRequiredCount; } }

    public int SweepRequiredCount
    {
        get
        {
            return (RideType == RideType.A30) ? 2 : 1;
        }
    }

    public bool SweepCountMet { get { return Coordinators[CoordinatorRole.Sweep].Count >= SweepRequiredCount; } }

}