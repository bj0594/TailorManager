namespace TailorManager.Core;

// A change the workshop asks the customer about (§4). It is Pending until RecordCustomerDecision answers it.
public class CustomerDecisionRequest
{
    public Guid Id { get; set; }
    public string ProposedWorkChange { get; set; } = "";
    public decimal? ProposedPriceAdjustment { get; set; }
    public DecisionStatus Status { get; set; }
    public Guid? ResponseCommunicationId { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}
