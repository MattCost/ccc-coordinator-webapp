using System.Text.Json.Serialization;

namespace CCC.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityTypes
{
    Unknown = 0,
    BikeRoute = 1,
    GroupRide = 2,
    RideEvent = 3,
}