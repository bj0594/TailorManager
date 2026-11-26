namespace TailorManager.Api;

// POST /orders/{orderId}/fittings
public record ScheduleFittingRequest(DateTimeOffset ScheduledAt);
