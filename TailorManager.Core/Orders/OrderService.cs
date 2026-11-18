using TailorManager.Core.Repository;

namespace TailorManager.Core;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;

    public OrderService(IOrderRepository repository, ICustomerRepository customerRepository)
    {
        _repository = repository;
        _customerRepository = customerRepository;
    }

    public Order AddOrder(Guid customerId, OrderType type, string workDescription, decimal agreedPrice, Guid? relatedOrderId, User employee)
    {
        var order = new Order { Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = type,
            WorkDescription = workDescription,
            InitialAgreedPrice = agreedPrice,
            RelatedOrderId = relatedOrderId,
            History = [new() { Id = Guid.NewGuid(), ActorId = employee.Id, ActorDisplayName = string.IsNullOrEmpty(employee.LastName) ? employee.FirstName : $"{employee.FirstName} {employee.LastName[0]}." }]
        };
        _repository.Add(order);
        return order;
    }

    public List<Order> GetOrders()
    {
        return _repository.GetAll().ToList();
    }

    public Order GetOrder(Guid id)
    {
        throw new NotImplementedException();
    }
}
