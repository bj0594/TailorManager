using TailorManager.Core.Repository;

namespace TailorManager.Core;

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }
    public Customer AddCustomer(string name, string phone, string email, string address)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            Email = email,
            Address = address
        };
        _repository.Add(customer);

        return customer;
    }

    public List<Customer> GetCustomers()
    {
        return _repository.GetAll();
    }
}
