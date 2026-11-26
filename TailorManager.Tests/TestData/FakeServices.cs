using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// The services with fake repositories that share their data, and the example values from §4.
// One line in Arrange makes what a test needs: var order = _app.AddOrder(); makes the customer and the order,
// with Employee as the logged-in employee.
public class FakeServices
{
    public FakeCustomerRepository CustomerRepository { get; } = new();
    public FakeOrderRepository OrderRepository { get; } = new();

    public CustomerService Customers { get; }
    public OrderService Orders { get; }

    // The logged-in employee the order methods get, shown as "Kari N." in the history.
    public User Employee { get; } = TestData.NewEmployee();

    // The clock the services read the time from. A test moves it with Clock.Advance(...).
    public TestClock Clock { get; } = new();

    public FakeServices()
    {
        Customers = new CustomerService(CustomerRepository);
        Orders = new OrderService(OrderRepository, CustomerRepository, Clock);
    }

    public Customer AddCustomer(string name = Examples.CustomerName) =>
        Customers.AddCustomer(name, Examples.CustomerPhone, Examples.CustomerEmail, Examples.CustomerAddress);

    // Makes a new customer for the order, unless the test gives the id of one.
    public Order AddOrder(Guid? customerId = null, string workDescription = Examples.WorkDescription, decimal agreedPrice = Examples.AgreedPrice) =>
        Orders.AddOrder(customerId ?? AddCustomer().Id, OrderType.NewGarment, workDescription, agreedPrice, null, Employee);

    // An order that is InProgress: made and started by Employee.
    public Order AddOrderInProgress(decimal agreedPrice = Examples.AgreedPrice)
    {
        var order = AddOrder(agreedPrice: agreedPrice);
        return Orders.StartOrder(order.Id, Employee);
    }

    // An order that is ReadyForPickup: made, started and marked by Employee.
    public Order AddOrderReadyForPickup(decimal agreedPrice = Examples.AgreedPrice) =>
        Orders.MarkReadyForPickup(AddOrderInProgress(agreedPrice).Id, Employee);
}
