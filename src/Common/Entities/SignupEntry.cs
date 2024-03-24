namespace CCC.Entities;

public class SignupEntry
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName {get;set;} = string.Empty;
    public Guid RideId { get; set; }
    public CoordinatorRole CoordinatorRole { get; set; }
}
