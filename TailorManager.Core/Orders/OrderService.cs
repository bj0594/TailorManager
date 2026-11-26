using TailorManager.Core.Repository;

namespace TailorManager.Core;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly TimeProvider _clock;

    // The clock is TimeProvider.System in the API, and a TestClock in the tests (§4, Dependencies).
    public OrderService(IOrderRepository repository, ICustomerRepository customerRepository, TimeProvider? clock = null)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _clock = clock ?? TimeProvider.System;
    }

    public Order AddOrder(Guid customerId, OrderType type, string workDescription, decimal agreedPrice, Guid? relatedOrderId, User employee)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = type,
            WorkDescription = workDescription,
            InitialAgreedPrice = agreedPrice,
            RelatedOrderId = relatedOrderId
        };
        AddHistory(order, employee, ActivityType.OrderCreated);
        _repository.Add(order);
        return order;
    }

    public List<Order> ListOrders(OrderStatus? status = null, Guid? customerId = null)
    {
        var query = _repository.GetAll().AsEnumerable();
        if (status is not null) query = query.Where(o => o.Status == status);
        if (customerId is not null) query = query.Where(o => o.CustomerId == customerId);
        return query.ToList();
    }
    public Order GetOrder(Guid id) =>
    _repository.GetById(id) ?? throw new KeyNotFoundException($"Order {id} was not found.");

    public Order StartOrder(Guid id, User employee)
    {
        var order = GetOrder(id);
        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOrderStatusException("The order is not Submitted.");
        }
        MoveTo(order, OrderStatus.InProgress, employee);
        return Save(order);
    }

    public Order AddWorkNote(Guid id, string note, User employee)
    {
        var order = GetOrder(id);
        AddHistory(order, employee, ActivityType.WorkNoteAdded, note);
        return Save(order);
    }

    // The customer is told the order is ready: a notification request in the history (§4). Nothing is sent from Core.
    public Order MarkReadyForPickup(Guid id, User employee)
    {
        var order = GetOrder(id);
        MoveTo(order, OrderStatus.ReadyForPickup, employee);
        AddHistory(order, employee, ActivityType.NotificationRequested);
        return Save(order);
    }

    // A full payment delivers the order (§2).
    public Order RegisterPayment(Guid id, decimal amount, string transactionReference, User employee)
    {
        var order = GetOrder(id);
        order.Payments.Add(new Payment { Id = Guid.NewGuid(), Amount = amount, TransactionReference = transactionReference });
        AddHistory(order, employee, ActivityType.PaymentReceived);
        MoveTo(order, OrderStatus.Delivered, employee);
        return Save(order);
    }

    public Order MarkDelivered(Guid id, User employee)
    {
        var order = GetOrder(id);
        MoveTo(order, OrderStatus.Delivered, employee);
        return Save(order);
    }

    // A new status, with a StatusChanged line in the history.
    private static void MoveTo(Order order, OrderStatus status, User employee)
    {
        order.Status = status;
        AddHistory(order, employee, ActivityType.StatusChanged);
    }

    // A new line in the history, by the employee who did it. The history is append-only.
    private static void AddHistory(Order order, User employee, ActivityType type, string? note = null)
    {
        var entry = OrderHistoryEntry.By(employee);
        entry.Type = type;
        entry.Note = note;
        order.History.Add(entry);
    }

    private Order Save(Order order)
    {
        _repository.Update(order);
        return order;
    }
}
