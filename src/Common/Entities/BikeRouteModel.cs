namespace CCC.Entities;

// TODO rename to BikeRouteModel is vscode helps
public class BikeRoute
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CueEntry> Cues { get; set; } = new();
    public double Distance { get; set; }

    public string RideWithGPSRouteId {get;set;} = string.Empty;
    public string GarminConnectRouteId {get;set;} = string.Empty;
    public string RideWithGPSLink => $"https://ridewithgps.com/routes/{RideWithGPSRouteId}";
    public string GarminConnectLink => $"https://connect.garmin.com/modern/course/{GarminConnectRouteId}";

    public string RideWithGpsEmbed  => string.IsNullOrWhiteSpace(RideWithGPSRouteId) ? string.Empty : $"""<iframe src="https://ridewithgps.com/embeds?type=route&id={RideWithGPSRouteId}&sampleGraph=true" style="width: 1px; min-width: 100%; height: 700px; border: none;" scrolling="no"></iframe>""";
}

