namespace TailorManager.Core;

// A change to the agreed price. It is only made when the customer approves it (§4, Calculated values).
public class PriceAdjustment
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTimeOffset ApprovedAt { get; set; }
    public Guid CustomerApprovalId { get; set; }
}
