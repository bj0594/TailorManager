namespace TailorManager.Web.Api;

// The customer's written agreement, as the API sends it (CustomerApproval in §4).
public record CustomerApprovalDto(Guid Id, string Scope, string Method, string? DocumentReference, decimal? ApprovedAmount);
