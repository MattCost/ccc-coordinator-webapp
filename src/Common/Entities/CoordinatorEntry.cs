namespace CCC.Entities;

// Rename to CoordinatorSlotEntry
public class CoordinatorEntry
{
    public List<string> CoordinatorIds { get; set; } = new();  // Graph API uses string so thats what we use
    public int RequiredCount { get; set; }
    public bool RequiredCountMet => CoordinatorIds.Count >= RequiredCount;
}
