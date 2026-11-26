namespace TailorManager.Web.Api;

// One line in the order's history, as the API sends it (OrderHistoryEntry in §4).
public record OrderHistoryDto(string Type, DateTimeOffset OccurredAt, string? ActorDisplayName, string? Note);
