namespace TailorManager.Web.Api;

// One order as the API sends it. The fields are the ones in the response in api.md.
// The lists come last, with null as the default, so the tests only give the ones they check.
public record OrderDto(Guid Id, Guid CustomerId, string Type, string WorkDescription, decimal InitialAgreedPrice, decimal CurrentAgreedPrice, decimal OutstandingBalance, string Status, DateTimeOffset CreatedAt, Guid? RelatedOrderId,
    List<OrderHistoryDto>? History = null, List<CustomerDecisionRequestDto>? CustomerDecisionRequests = null, List<FittingDto>? Fittings = null);
