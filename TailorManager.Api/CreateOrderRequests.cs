using TailorManager.Core;

namespace TailorManager.Api;

public record CreateOrderRequest(
    Guid CustomerId, 
    OrderType Type, 
    string WorkDescription, 
    decimal AgreedPrice, 
    Guid? RelatedOrderId
);
