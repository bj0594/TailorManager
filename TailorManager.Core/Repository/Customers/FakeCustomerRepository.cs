namespace TailorManager.Core.Repository;

// Keeps the customers in memory, for the tests.
public class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();

    public void Add(Customer customer) => _customers.Add(customer);
    public List<Customer> GetAll() => _customers;
    public Customer? GetById(Guid id) => _customers.FirstOrDefault(c => c.Id == id);

    // The objects in the list are the ones the services change, so there is nothing to save.
    public void Update(Customer customer)
    {
    }
}
