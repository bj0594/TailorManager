namespace TailorManager.Web.Api;

// What the pages can ask the API for. OrdersClient calls the API, and FakeOrdersClient is used in the tests.
public interface IOrdersClient
{
    Task<List<OrderDto>> GetOrdersAsync();
    Task AddOrderAsync(
        Guid Id, 
        Guid CustomerId, 
        string Type, 
        string WorkDescription, 
        decimal InitialAgreedPrice, 
        decimal CurrentAgreedPrice, 
        decimal OutstandingBalance, 
        string Status, 
        DateTimeOffset CreatedAt, 
        Guid? RelatedOrderId
    );

    Task<OrderDto> GetOrderAsync(Guid id);

    Task AddWorkNoteAsync(Guid id, string note);

    Task StartOrderAsync(Guid id);

    Task MarkOrderReadyForPickupAsync(Guid id);

    Task AddPaymentAsync(Guid id, decimal amount);
}
