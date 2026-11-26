namespace TailorManager.Web.Api;

// An email or SMS to or from the customer, as the API sends it (CommunicationEntry in §4).
public record CommunicationDto(Guid Id, string Channel, string Direction, string Source, string Content, string Outcome, DateTimeOffset OccurredAt);

// What the page sends to record one: the body of POST /orders/{orderId}/communications in api.md.
public record CommunicationInput(string Channel, string Direction, string Source, string Content, string Outcome, DateTimeOffset OccurredAt);
