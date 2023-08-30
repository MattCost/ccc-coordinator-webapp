namespace CCC.Entities;

public class RideEventUpdateModel
{
    public string? Name { get; set; } 
    public string? Description { get; set; } 
    public DateTimeOffset? StartTime { get; set; }
    public string? Location { get; set; }

}