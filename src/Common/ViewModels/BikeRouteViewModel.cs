using CCC.Entities;

namespace CCC.ViewModels;

public class BikeRouteViewModel : BikeRoute
{
    public bool IsFavorite { get; set; }

    public BikeRouteViewModel() {}
    public BikeRouteViewModel(BikeRoute model)
    {
        //sure would be nice if i had a base class with a clone method
        Id = model.Id;
        Name = model.Name;
        Description = model.Description;
        Cues = model.Cues;
        Distance = model.Distance;
        RideWithGPSRouteId = model.RideWithGPSRouteId;
        GarminConnectRouteId = model.GarminConnectRouteId;
        
    }
}