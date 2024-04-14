namespace CCC.Entities;

public class RideEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<Guid> RideIds { get; set; } = new();
    public Dictionary<CoordinatorRole, CoordinatorEntry> SupportPersonnel { get; set; } = new();
    public Enums.EventTypes EventType {get; set; }
}