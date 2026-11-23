using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeCustomersClient : ICustomersClient
{
    public List<CustomerDto> Items { get; } = [];

    public Task<List<CustomerDto>> GetCustomersAsync() => Task.FromResult(Items);
    public Task<CustomerDto> AddCustomerAsync(string name, string phone, string email, string address)
    {
        var customer = new CustomerDto(Guid.NewGuid(), name, phone, email, address);
        Items.Add(customer);
        return Task.FromResult(customer);
    }

    public Task<CustomerDto> GetCustomerAsync(Guid id) => Task.FromResult(Items.Single(i => i.Id == id));
}
