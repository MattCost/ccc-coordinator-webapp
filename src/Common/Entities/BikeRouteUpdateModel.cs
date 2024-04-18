namespace CCC.Entities;

public class BikeRouteUpdateModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Distance { get; set; } = null;
    public string? RideWithGPSRouteId { get; set; }
    public string? RideWithGPSLink { get; set; }
    public string? GarminConnectLink { get; set; }

}

