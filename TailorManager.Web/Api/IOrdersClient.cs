namespace TailorManager.Web.Api;

// What the pages can ask the API for. OrdersClient calls the API, and FakeOrdersClient is used in the tests.
// The actions have the names in §5, and throw HttpRequestException with the API's message when the API says no.
public interface IOrdersClient
{
    Task<List<OrderDto>> ListOrdersAsync(string? status = null, Guid? customerId = null); 
    Task<OrderDto> GetOrderAsync(Guid id);
    Task<OrderDto> CreateOrderAsync(Guid customerId, string type, string workDescription, decimal agreedPrice);
    Task StartWorkAsync(Guid id);
    Task AddWorkNoteAsync(Guid id, string note);
    Task MarkReadyForPickupAsync(Guid id);
    Task RequestCustomerDecisionAsync(Guid id, string proposedWorkChange, decimal? proposedPriceAdjustment);
    Task RecordCustomerDecisionAsync(Guid id, Guid requestId, Guid communicationId, bool approved);
    Task ScheduleFittingAsync(Guid id, DateTimeOffset scheduledAt);
    Task CompleteFittingAsync(Guid id, Guid fittingId, string outcome);
    Task RecordCommunicationAsync(Guid id, CommunicationInput communication);
    Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference);
    Task MarkDeliveredAsync(Guid id);
}
