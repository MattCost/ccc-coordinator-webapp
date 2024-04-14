namespace CCC.Entities;

public class RideEventUpdateModel
{
    public string? Name { get; set; } 
    public string? Description { get; set; } 
    public DateTimeOffset? StartTime { get; set; }
    public string? Location { get; set; }
    public bool? GrillMaster {get;set;}
    public bool? Facilitator {get;set;}
    public Enums.EventTypes? EventType {get; set; } 

}