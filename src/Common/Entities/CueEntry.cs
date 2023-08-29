namespace CCC.Entities;

public enum CueOperation
{
    StartRoute = 0,
    Left = 1,
    Right = 2,
    Cross = 3,
    Straight = 4,
    LeftAt = 5,
    RightAt = 6,
    SlightLeft = 7,
    SlightRight = 8,
    LeftCircle = 9,
    RightCircle = 10,
    StraightCircle = 11,
    EndRoute = 12
}

public class CueEntry
{
    public CueOperation Operation { get; set; }
    public string StreetName { get; set; } = string.Empty;
    public string Notes {get;set; } = string.Empty;
    public double MileMarker { get; set; }
}
