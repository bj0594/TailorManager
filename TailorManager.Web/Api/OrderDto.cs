namespace TailorManager.Web.Api;

// One order as the API sends it. The fields are the ones in the response in api.md.
public record OrderDto(Guid Id, Guid CustomerId, string Type, string WorkDescription, decimal InitialAgreedPrice, decimal CurrentAgreedPrice, decimal OutstandingBalance, string Status, DateTimeOffset CreatedAt, Guid? RelatedOrderId);
