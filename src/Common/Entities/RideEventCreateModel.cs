namespace CCC.Entities;

public class RideEventCreateModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow.Date;
    public string Location { get; set; } = string.Empty;
    public List<Guid> Rides { get; set; } = new();
    public bool Facilitator { get; set; }
    public bool GrillMaster { get; set; }
    public EventTypes EventType {get; set; } 

}