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
            History = [OrderHistoryEntry.By(employee)]
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
        return _repository.GetById(id)
        ?? throw new KeyNotFoundException($"Order {id} was not found.");
    }

    public void StartOrder(Guid id)
    {
        var order = GetOrder(id);
        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOrderStatusException("The order is not Submitted.");
        }
        order.Status = OrderStatus.InProgress;
        _repository.Update(order);
    }
}
