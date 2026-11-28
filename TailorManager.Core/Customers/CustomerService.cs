using TailorManager.Core.Repository;

namespace TailorManager.Core;

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository) => _repository = repository;

    // A customer needs all four details (§6).
    public Customer AddCustomer(string name, string phone, string email, string address)
    {
        Require(name, "name");
        Require(phone, "phone");
        Require(email, "email");
        Require(address, "address");
        var customer = new Customer { Id = Guid.NewGuid(), Name = name, Phone = phone, Email = email, Address = address };
        _repository.Add(customer);
        return customer;
    }

    public List<Customer> GetCustomers() => _repository.GetAll();

    public List<Customer> FindCustomers(string searchTerm) =>
        _repository.GetAll()
            .Where(c => c.Name == searchTerm || c.Phone == searchTerm || c.Email == searchTerm)
            .ToList();
            
    // The message names the field, so the website can show which one is missing.
    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"The {field} is required.");
        }
    }

    public Customer GetCustomer(Guid id) =>
        _repository.GetById(id) ?? throw new KeyNotFoundException($"Customer {id} was not found.");
}
