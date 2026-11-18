namespace TailorManager.Core;

public class OrderHistoryEntry
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorDisplayName { get; set; }
}
