using TailorManager.Core;

namespace TailorManager.Api;

// POST /orders/{orderId}/approvals
public record RecordInitialApprovalRequest(ApprovalMethod Method, string? DocumentReference);
