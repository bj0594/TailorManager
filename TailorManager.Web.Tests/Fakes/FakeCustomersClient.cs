using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeCustomersClient : ICustomersClient
{
    public List<CustomerDto> Items { get; } = [];

    public Task<CustomerDto> GetCustomerAsync(Guid id) => Task.FromResult(Items.Single(i => i.Id == id));
    public Task<CustomerDto> AddCustomerAsync(string name, string phone, string email, string address)
    {
        var customer = new CustomerDto(Guid.NewGuid(), name, phone, email, address);
        Items.Add(customer);
        return Task.FromResult(customer);
    }

    public Task<List<CustomerDto>> FindCustomersAsync(string? searchTerm = null) =>
    Task.FromResult(Items
        .Where(c => searchTerm is null || c.Name == searchTerm || c.Phone == searchTerm || c.Email == searchTerm)
        .ToList());
}
