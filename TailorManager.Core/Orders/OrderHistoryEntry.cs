namespace TailorManager.Core;

// One line in an order's history (§4). OrderService adds them through AddHistory, and never changes one.
public class OrderHistoryEntry
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorDisplayName { get; set; }
    public ActivityType Type { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? Note { get; set; }

    // The detailed record the line is about, like the decision request (§4, History).
    public Guid? RelatedEntityId { get; set; }

    // A new line by the employee who did it, shown as "Kari N.".
    public static OrderHistoryEntry By(User employee) => new()
    {
        Id = Guid.NewGuid(),
        ActorId = employee.Id,
        ActorDisplayName = employee.DisplayName
    };
}
