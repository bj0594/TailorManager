namespace TailorManager.Api;

// POST /orders/{orderId}/payments
public record RegisterPaymentRequest(decimal Amount, string TransactionReference);
