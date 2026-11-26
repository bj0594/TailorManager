using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// The same tests for every implementation of IOrderRepository.
public abstract class OrderRepositoryContractTests
{
    protected abstract IOrderRepository CreateRepository();

    // Reads through a new repository, so nothing comes from what EF remembers.
    protected virtual IOrderRepository Reopen(IOrderRepository repository) => repository;

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();

        // Act
        repository.Add(order);

        // Assert
        var found = repository.GetById(order.Id);
        Assert.NotNull(found);
        Assert.Equal(Examples.WorkDescription, found.WorkDescription);
    }

    [Fact]
    public void Add_ThenGetById_KeepsTheHistory()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        order.History.Add(OrderHistoryEntry.By(TestData.NewEmployee()));

        // Act
        repository.Add(order);

        // Assert
        var stored = Reopen(repository).GetById(order.Id)!;
        var entry = Assert.Single(stored.History);
        Assert.Equal(Examples.EmployeeDisplayName, entry.ActorDisplayName);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var found = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void GetAll_AfterTwoAdded_ContainsBoth()
    {
        // Arrange
        var repository = CreateRepository();
        var first = TestData.NewOrder();
        var second = TestData.NewOrder(workDescription: Examples.OtherWorkDescription);
        repository.Add(first);
        repository.Add(second);

        // Act
        var all = repository.GetAll();

        // Assert
        Assert.Contains(all, o => o.Id == first.Id);
        Assert.Contains(all, o => o.Id == second.Id);
    }

    [Fact]
    public void Update_ChangedOrder_IsStored()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        repository.Add(order);

        // Act
        order.Status = OrderStatus.InProgress;
        repository.Update(order);

        // Assert
        Assert.Equal(OrderStatus.InProgress, Reopen(repository).GetById(order.Id)!.Status);
    }

    [Fact]
    public void Update_NewPayment_IsStored()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        repository.Add(order);

        // Act
        order.Payments.Add(new Payment { Id = Guid.NewGuid(), Amount = Examples.PaymentAmount, TransactionReference = Examples.TransactionReference });
        repository.Update(order);

        // Assert
        var payment = Assert.Single(Reopen(repository).GetById(order.Id)!.Payments);
        Assert.Equal(Examples.PaymentAmount, payment.Amount);
        Assert.Equal(Examples.TransactionReference, payment.TransactionReference);
    }

    [Fact]
    public void Update_NewHistoryEntry_IsStored()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        repository.Add(order);

        // Act
        var entry = OrderHistoryEntry.By(TestData.NewEmployee());
        entry.Type = ActivityType.StatusChanged;
        order.History.Add(entry);
        repository.Update(order);

        // Assert
        var stored = Reopen(repository).GetById(order.Id)!;
        Assert.Equal(ActivityType.StatusChanged, Assert.Single(stored.History).Type);
    }

    [Fact]
    public void GetAll_WithStatusAndCustomer_ReturnsOnlyMatching()
    {
        // Arrange
        var repository = CreateRepository();
        var customerId = Guid.NewGuid();
        var match = TestData.NewOrder(customerId);
        match.Status = OrderStatus.InProgress;
        var otherStatus = TestData.NewOrder(customerId, Examples.OtherWorkDescription);
        var otherCustomer = TestData.NewOrder();
        otherCustomer.Status = OrderStatus.InProgress;
        repository.Add(match);
        repository.Add(otherStatus);
        repository.Add(otherCustomer);

        // Act
        var found = Reopen(repository).GetAll(OrderStatus.InProgress, customerId);

        // Assert
        Assert.Equal(match.Id, Assert.Single(found).Id);
    }

    [Fact]
    public void GetById_HistoryAddedOutOfOrder_ReturnsItOldestFirst()
    {
        // Arrange: the later entry is added first
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        var createdAt = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);
        order.History.Add(new OrderHistoryEntry { Id = Guid.NewGuid(), Type = ActivityType.StatusChanged, OccurredAt = createdAt.AddHours(1) });
        order.History.Add(new OrderHistoryEntry { Id = Guid.NewGuid(), Type = ActivityType.OrderCreated, OccurredAt = createdAt });
        repository.Add(order);

        // Act
        var found = Reopen(repository).GetById(order.Id);

        // Assert
        Assert.Equal(new[] { ActivityType.OrderCreated, ActivityType.StatusChanged }, found!.History.Select(h => h.Type));
    }

    [Fact]
    public void Update_NewCustomerDecisionRequest_IsStored()
    {
        // Arrange
        var repository = CreateRepository();
        var order = TestData.NewOrder();
        repository.Add(order);

        // Act
        order.CustomerDecisionRequests.Add(new CustomerDecisionRequest { Id = Guid.NewGuid(), ProposedWorkChange = Examples.ProposedWorkChange, ProposedPriceAdjustment = Examples.ProposedPriceAdjustment });
        repository.Update(order);

        // Assert
        var request = Assert.Single(Reopen(repository).GetById(order.Id)!.CustomerDecisionRequests);
        Assert.Equal(Examples.ProposedWorkChange, request.ProposedWorkChange);
        Assert.Equal(Examples.ProposedPriceAdjustment, request.ProposedPriceAdjustment);
        Assert.Equal(DecisionStatus.Pending, request.Status);
    }
}

public class FakeOrderRepositoryTests : OrderRepositoryContractTests
{
    protected override IOrderRepository CreateRepository() => new FakeOrderRepository();
}
