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
    public bool IsReadyForPickup { get; set; }

    public List<Payment> Payments { get; set; } = [];
    public decimal OutstandingBalance =>
    InitialAgreedPrice - Payments.Sum(payment => payment.Amount);
    public bool IsDelivered { get; set; }
}
