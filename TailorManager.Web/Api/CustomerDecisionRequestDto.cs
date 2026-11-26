namespace TailorManager.Web.Api;

// A change the workshop asked the customer about, as the API sends it (CustomerDecisionRequest in §4).
public record CustomerDecisionRequestDto(Guid Id, string ProposedWorkChange, decimal? ProposedPriceAdjustment, string Status);
