namespace TailorManager.Api;

// PATCH /orders/{orderId}/fittings/{fittingId}/complete
public record CompleteFittingRequest(string Outcome);
