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

    public Order StartOrder(Guid id, User employee)
    {
        var order = GetOrder(id);
        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOrderStatusException("The order is not Submitted.");
        }
        order.Status = OrderStatus.InProgress;
        AddHistory(order, employee, ActivityType.StatusChanged);
        _repository.Update(order);
        return order;
    }

    public Order AddWorkNote(Guid id, string note, User employee)
    {
        var order = GetOrder(id);
        AddHistory(order, employee, ActivityType.WorkNoteAdded, note);
        _repository.Update(order);
        return order;
    }

    public Order MarkReadyForPickup(Guid id, User employee)
    {
        var order = GetOrder(id);
        order.Status = OrderStatus.ReadyForPickup;
        AddHistory(order, employee, ActivityType.StatusChanged);
        AddHistory(order, employee, ActivityType.NotificationRequested);
        _repository.Update(order);
        return order;
    }

    public Order RegisterPayment(Guid id, decimal amount, string transactionReference, User employee)
    {
        var order = GetOrder(id);
        order.Payments.Add(new Payment { Id = Guid.NewGuid(), Amount = amount, TransactionReference = transactionReference });
        AddHistory(order, employee, ActivityType.PaymentReceived);
        order.Status = OrderStatus.Delivered;
        AddHistory(order, employee, ActivityType.StatusChanged);
        _repository.Update(order);
        return order;
    }

    public void DeliverOrder(Guid id)
    {
        var order = GetOrder(id);
        order.IsDelivered = true;
        _repository.Update(order);
    }

    // A new line in the history, by the employee who did it. The history is append-only.
    private static void AddHistory(Order order, User employee, ActivityType type, string? note = null)
    {
        var entry = OrderHistoryEntry.By(employee);
        entry.Type = type;
        entry.Note = note;
        order.History.Add(entry);
    }
}
