namespace TailorManager.Core;

// The customer's written agreement (§4): to the order, to a change, or to an inspection fee.
public class CustomerApproval
{
    public Guid Id { get; set; }
    public ApprovalScope Scope { get; set; }
    public ApprovalMethod Method { get; set; }
    public DateTimeOffset ApprovedAt { get; set; }
    public Guid? RelatedDecisionRequestId { get; set; }
    public Guid? CommunicationId { get; set; }
    public string? DocumentReference { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? ApprovedTerms { get; set; }
}
