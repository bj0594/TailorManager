namespace TailorManager.Core;

public enum OrderStatus
{
    Submitted,
    InProgress,
    WaitingForCustomer,
    WaitingForFitting,
    ReadyForPickup,
    Delivered
}
