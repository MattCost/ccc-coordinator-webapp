namespace CCC.Entities;

public class RideEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<int> Rides { get; set; } = new();

}