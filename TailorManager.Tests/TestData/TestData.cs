using TailorManager.Core;

namespace TailorManager.Tests;

// New objects with the example values, for tests that build an object without a service (like the repository tests).
// Change a value with an argument: TestData.NewOrder(workDescription: Examples.OtherWorkDescription).
public static class TestData
{
    // The communication in §4: an email from the customer, received now.
    public static CommunicationEntry NewCommunication(
        CommunicationChannel channel = CommunicationChannel.Email,
        CommunicationDirection direction = CommunicationDirection.Incoming,
        CommunicationOutcome outcome = CommunicationOutcome.Received) => new()
    {
        Id = Guid.NewGuid(),
        Channel = channel,
        Direction = direction,
        Source = direction == CommunicationDirection.Incoming ? CommunicationSource.Customer : CommunicationSource.Manual,
        Content = Examples.CommunicationContent,
        Outcome = outcome,
        OccurredAt = new DateTimeOffset(2026, 11, 3, 14, 0, 0, TimeSpan.Zero)
    };
    public static Customer NewCustomer(string name = Examples.CustomerName) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Phone = Examples.CustomerPhone,
        Email = Examples.CustomerEmail,
        Address = Examples.CustomerAddress
    };

    // Without a customerId, the order gets a new id that no customer has. The repository tests do not check it.
    public static Order NewOrder(Guid? customerId = null, string workDescription = Examples.WorkDescription, decimal agreedPrice = Examples.AgreedPrice) => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = customerId ?? Guid.NewGuid(),
        Type = OrderType.NewGarment,
        WorkDescription = workDescription,
        InitialAgreedPrice = agreedPrice
    };

    // The logged-in employee the order methods take last. The username is new every time, so two employees never collide.
    public static User NewEmployee(string role = Roles.FrontDesk) => new()
    {
        Id = Guid.NewGuid(),
        Username = $"employee-{Guid.NewGuid()}",
        FirstName = Examples.EmployeeFirstName,
        LastName = Examples.EmployeeLastName,
        Role = role
    };
}
