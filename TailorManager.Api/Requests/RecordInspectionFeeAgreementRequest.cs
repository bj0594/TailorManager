namespace TailorManager.Api;

// POST /orders/{orderId}/inspection-fee-agreements
public record RecordInspectionFeeAgreementRequest(decimal Amount, string DocumentReference);
