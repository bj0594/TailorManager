using TailorManager.Core;

namespace TailorManager.Api;

// POST /orders/{orderId}/communications
public record RecordCommunicationRequest(
    CommunicationChannel Channel,
    CommunicationDirection Direction,
    CommunicationSource Source,
    string Content,
    CommunicationOutcome Outcome,
    DateTimeOffset OccurredAt);
