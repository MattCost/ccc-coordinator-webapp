using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CCC.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventTypes
{
    [Display(Name = "Unknown")]
    Unknown = 0,

    [Display(Name = "Road Ride")]
    Road = 1,

    [Display(Name = "GravGrav")]
    
    Gravel = 2,

    [Display(Name = "Gnar")]
    MTB = 3,
}