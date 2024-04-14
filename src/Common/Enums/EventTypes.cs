using System.Text.Json.Serialization;

namespace CCC.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventTypes
{
    Unknown = 0,
    Road = 1,
    Gravel = 2,
    MTB = 3,
}