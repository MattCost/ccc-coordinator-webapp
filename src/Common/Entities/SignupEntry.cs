using Microsoft.Graph.Models;

namespace CCC.Entities;

public class SignupEntry
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Enums.EntityTypes EntityType { get; set; }
    public CoordinatorRole CoordinatorRole { get; set; }
}
