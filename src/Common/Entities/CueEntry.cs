namespace CCC.Entities;

using System.Text.Json.Serialization;
using CCC.Enums;


public class CueEntry
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CueOperation Operation { get; set; }
    // public string StreetName { get; set; } = string.Empty;
    // public string Notes {get;set; } = string.Empty;
    public double MileMarker { get; set; } 
    public string Description {get;set;} = string.Empty;
}
