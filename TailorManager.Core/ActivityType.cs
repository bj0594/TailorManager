namespace TailorManager.Core;

public enum ActivityType
{
    OrderCreated,
    InitialApprovalRecorded,
    StatusChanged,
    WorkNoteAdded,
    CustomerDecisionRequested,
    CustomerDecisionRecorded,
    FittingScheduled,
    FittingCompleted,
    CommunicationRecorded,
    PriceAdjusted,
    InspectionFeeAgreementRecorded,
    NotificationRequested,
    PaymentReceived
}
