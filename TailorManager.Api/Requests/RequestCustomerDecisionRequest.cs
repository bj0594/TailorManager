namespace TailorManager.Api;

// POST /orders/{orderId}/decision-requests
public record RequestCustomerDecisionRequest(string ProposedWorkChange, decimal? ProposedPriceAdjustment);
