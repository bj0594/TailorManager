using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeCustomersClient : ICustomersClient
{
    public List<CustomerDto> Items { get; } = [];

    public Task<List<CustomerDto>> GetCustomersAsync() => Task.FromResult(Items);
    public Task AddCustomerAsync(string name, string email, string phoneNumber, string address)
    {
        Items.Add(new CustomerDto(Guid.NewGuid(), name, email, phoneNumber, address));
        return Task.CompletedTask;
    }
}
