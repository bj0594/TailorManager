namespace TailorManager.Core;

public class OrderHistoryEntry
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorDisplayName { get; set; }

    // A new line by the employee who did it. Every action on an order adds one: order.History.Add(OrderHistoryEntry.By(employee)).
    public static OrderHistoryEntry By(User employee) => new()
    {
        Id = Guid.NewGuid(),
        ActorId = employee.Id,
        ActorDisplayName = employee.DisplayName
    };
}
