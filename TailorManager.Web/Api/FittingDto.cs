namespace TailorManager.Web.Api;

// A fitting with the customer, as the API sends it (FittingAppointment in §4).
public record FittingDto(Guid Id, DateTimeOffset ScheduledAt, DateTimeOffset? CompletedAt, string? Outcome);
