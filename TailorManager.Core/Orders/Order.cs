namespace TailorManager.Core;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderType Type { get; set; }
    public string WorkDescription { get; set; } = "";
    public decimal InitialAgreedPrice { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public List<OrderHistoryEntry> History { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<CustomerDecisionRequest> CustomerDecisionRequests { get; set; } = [];
    public List<CustomerApproval> CustomerApprovals { get; set; } = [];
    public List<FittingAppointment> Fittings { get; set; } = [];
    public List<PriceAdjustment> ApprovedPriceAdjustments { get; set; } = [];

    // Calculated (§4): the agreed price with the approved adjustments. Not stored.
    public decimal CurrentAgreedPrice => InitialAgreedPrice + ApprovedPriceAdjustments.Sum(adjustment => adjustment.Amount);

    // Calculated (§4): the current agreed price minus what is paid. Not stored.
    public decimal OutstandingBalance => CurrentAgreedPrice - Payments.Sum(payment => payment.Amount);
}
