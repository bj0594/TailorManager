namespace TailorManager.Core;

public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; } = "";
}
