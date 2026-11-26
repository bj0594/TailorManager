namespace TailorManager.Api;

// POST /orders/{orderId}/decision-requests/{requestId}/decision
public record RecordCustomerDecisionRequest(Guid CommunicationId, bool Approved);
