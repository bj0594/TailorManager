namespace TailorManager.Core;

// An email or SMS to or from the customer (§4). The customer's written answers to decision requests are entries like this.
public class CommunicationEntry
{
    public Guid Id { get; set; }
    public CommunicationChannel Channel { get; set; }
    public CommunicationDirection Direction { get; set; }
    public CommunicationSource Source { get; set; }
    public string Content { get; set; } = "";
    public CommunicationOutcome Outcome { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
