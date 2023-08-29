namespace CCC.Entities;

// TODO rename to BikeRouteModel is vscode helps
public class BikeRoute
{
    public Guid Id { get; set;}
    public string Name {get;set;} = string.Empty;
    public string Description {get;set;} =  string.Empty;
    public List<CueEntry> Cues { get; set; } = new();
    public double Distance {get; set;}
}

