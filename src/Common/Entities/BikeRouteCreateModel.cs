namespace CCC.Entities;

public class BikeRouteCreateModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Distance { get; set; }
    public string? RideWithGPSRouteId { get; set; } = null;
    public string? GarminConnectRouteId { get; set; } = null;

}