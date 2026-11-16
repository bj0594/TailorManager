namespace TailorManager.Web.Api;

// What the pages can ask the API for. CustomersClient calls the API, and FakeCustomersClient is used in the tests.
public interface ICustomersClient
{
    Task<List<CustomerDto>> GetCustomersAsync();
}
