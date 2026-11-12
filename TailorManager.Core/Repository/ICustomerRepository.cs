namespace TailorManager.Core.Repository;

public interface ICustomerRepository
{
    void Add(Customer customer);
    List<Customer> GetAll();
    Customer? GetById(Guid id);
}
