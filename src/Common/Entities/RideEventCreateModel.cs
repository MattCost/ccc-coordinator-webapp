namespace CCC.Entities;

public class RideEventCreateModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<Guid> Rides { get; set; } = new();

}