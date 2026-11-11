namespace TailorManager.Core;

public class CustomerService
{
    private readonly List<Customer> _customer = new();
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
        _customer.Add(customer);

        return customer;
    }

    public List<Customer> GetCustomers()
    {
        return _customer.ToList();
    }
}
