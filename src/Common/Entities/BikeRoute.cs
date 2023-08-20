namespace CCC.Entities;

public class BikeRoute
{
    public int Id { get; set;}
    public string Name {get;set;} = string.Empty;
    public string Description {get;set;} =  string.Empty;
    public List<CueEntry> Cues { get; set; } = new();
    public double Distance {get; set;}
}
