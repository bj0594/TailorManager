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
            RelatedOrderId = relatedOrderId
         };
        _repository.Add(order);
        return order;
    }

    public List<Order> GetOrders()
    {
        return _repository.GetAll().ToList();
    }
}
