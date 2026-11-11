namespace TailorManager.Core.Repository;

public class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new ();

    public void Add(Customer customer) => _customers.Add(customer);
public List<Customer> GetAll() => _customers;
}
