namespace TailorManager.Core;

// A fitting with the customer (§4). It is open until CompleteFitting records the outcome.
public class FittingAppointment
{
    public Guid Id { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Outcome { get; set; }
}
