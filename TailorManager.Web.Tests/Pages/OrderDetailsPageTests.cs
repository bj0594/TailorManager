using System.Globalization;
using Bunit;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

public class OrderDetailsPageTests : PageTestContext
{
    [Fact]
    public void OrderDetailsPage_ExistingId_ShowsIt()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        var page = RenderOrder(order);

        // Assert
        Assert.Contains(Examples.WorkDescription, page.Find("h1").TextContent);
    }

    [Fact]
    public void OrderDetailsPage_ExistingId_ShowsTheStatus()
    {
        // Arrange
        var page = RenderOrder(TestDtos.NewOrder());

        // Assert
        Assert.Contains("Submitted", page.Find(".status").TextContent);
    }

    [Fact]
    public void OrderDetailsPage_ExistingId_ShowsTheCustomer()
    {
        // Arrange
        var page = RenderOrder(TestDtos.NewOrder());

        // Assert
        var customer = page.Find(".customer").TextContent;
        Assert.Contains(Examples.CustomerName, customer);
        Assert.Contains(Examples.CustomerPhone, customer);
    }

    [Fact]
    public void OrderDetailsPage_ClickStart_SendsItToTheApi()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder();
        var page = RenderOrder(order);

        // Act
        page.Find("button.start").Click();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains(order.Id, _orders.StartIds);
            Assert.Contains("InProgress", page.Find(".status").TextContent);
        });
    }

    // Each action is shown only to the role that may do it (§4). The test checks both roles,
    // so a selector the page does not have cannot make it green. A new action is one more line here.
    [Theory]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "button.start")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "form.note")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "button.readyforpickup")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "form.decisionrequest")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "button.approve")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "button.reject")]
    [InlineData(Roles.Workshop, Roles.FrontDesk, "form.fitting")]
    [InlineData(Roles.FrontDesk, Roles.Workshop, "form.payment")]
    [InlineData(Roles.FrontDesk, Roles.Workshop, "button.deliver")]
    public void OrderDetailsPage_OtherRole_HidesTheAction(string role, string otherRole, string action)
    {
        // Arrange: with a pending request, so the buttons for the decision are there too
        var order = TestDtos.NewOrder() with { CustomerDecisionRequests = [TestDtos.NewDecisionRequest()] };

        // Act and assert: the role sees it
        _tokens.Role = role;
        Assert.NotEmpty(RenderOrder(order).FindAll(action));

        // Act and assert: the other role does not
        _tokens.Role = otherRole;
        Assert.Empty(Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id)).FindAll(action));
    }

    [Fact]
    public void OrderDetailsPage_StartRejected_ShowsMessage()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder();
        _orders.Fail = new HttpRequestException("The order is not Submitted.");
        var page = RenderOrder(order);

        // Act
        page.Find("button.start").Click();

        // Assert
        page.WaitForAssertion(() =>
            Assert.Contains("The order is not Submitted.", page.Find(".error").TextContent));
    }

    [Fact]
    public void OrderDetailsPage_SubmitWorkNote_SendsItToTheApi()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        var page = RenderOrder(order);

        // Act
        page.Find("form.note input").Change(Examples.WorkNote);
        page.Find("form.note").Submit();

        // Assert
        page.WaitForAssertion(() =>
            Assert.Contains((order.Id, Examples.WorkNote), _orders.AddedWorkNotes));
    }

    [Fact]
    public void OrderDetailsPage_ClickReadyForPickup_SendsItToTheApi()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        var page = RenderOrder(order);

        // Act
        page.Find("button.readyforpickup").Click();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains(order.Id, _orders.ReadyForPickupIds);
            Assert.Contains("ReadyForPickup", page.Find(".status").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_SubmitPayment_SendsItToTheApi()
    {
        // Arrange
        _tokens.Role = Roles.FrontDesk;
        var order = TestDtos.NewOrder() with
        {
            Status = "ReadyForPickup",
            OutstandingBalance = Examples.PaymentAmount
        };
        var page = RenderOrder(order);

        // Act
        page.Find("form.payment input.amount").Change(Examples.PaymentAmount.ToString());
        page.Find("form.payment input.reference").Change(Examples.TransactionReference);
        page.Find("form.payment").Submit();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains((order.Id, Examples.PaymentAmount, Examples.TransactionReference), _orders.Payments);
            Assert.Contains("Delivered", page.Find(".status").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_FreeOrder_ShowsZeroOutstandingBalance()
    {
        // Arrange
        var order = TestDtos.NewOrder() with
        {
            OutstandingBalance = 0m
        };

        var page = RenderOrder(order);

        // Assert
        Assert.Contains(
            "Outstanding balance: 0",
            page.Find(".outstanding-balance").TextContent);
    }

    [Fact]
    public void OrderDetailsPage_ClickDeliver_SendsItToTheApi()
    {
        // Arrange
        _tokens.Role = Roles.FrontDesk;
        var order = TestDtos.NewOrder() with { Status = "ReadyForPickup", OutstandingBalance = 0m };
        var page = RenderOrder(order);

        // Act
        page.Find("button.deliver").Click();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains(order.Id, _orders.DeliveredIds);
            Assert.Contains("Delivered", page.Find(".status").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_WithHistory_ShowsItOldestFirst()
    {
        // Arrange: the API's list in the wrong order, so the test shows the page sorts it
        var createdAt = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);
        var order = TestDtos.NewOrder() with
        {
            History =
            [
                new("StatusChanged", createdAt.AddHours(1), "Ola N.", null),
                new("OrderCreated", createdAt, Examples.EmployeeDisplayName, null)
            ]
        };

        // Act
        var page = RenderOrder(order);

        // Assert
        var entries = page.FindAll("ul.history li");
        Assert.Equal(2, entries.Count);
        Assert.Contains("OrderCreated", entries[0].TextContent);
        Assert.Contains(Examples.EmployeeDisplayName, entries[0].TextContent);
        Assert.Contains("StatusChanged", entries[1].TextContent);
    }

    [Fact]
    public void OrderDetailsPage_SubmitDecisionRequest_ShowsWaitingForCustomer()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        var page = RenderOrder(order);

        // Act
        page.Find("form.decisionrequest input.workchange").Change(Examples.ProposedWorkChange);
        page.Find("form.decisionrequest input.priceadjustment").Change(Examples.ProposedPriceAdjustment.ToString());
        page.Find("form.decisionrequest").Submit();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains((order.Id, Examples.ProposedWorkChange, (decimal?)Examples.ProposedPriceAdjustment), _orders.DecisionRequests);
            Assert.Contains("WaitingForCustomer", page.Find(".status").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_ApproveChange_ShowsTheNewPrice()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var request = TestDtos.NewDecisionRequest();
        var order = TestDtos.NewOrder() with { Status = "WaitingForCustomer", CustomerDecisionRequests = [request] };
        var page = RenderOrder(order);

        // Act
        page.Find("button.approve").Click();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains(_orders.Decisions, d => d.Id == order.Id && d.RequestId == request.Id && d.Approved);
            Assert.Contains((Examples.AgreedPrice + Examples.ProposedPriceAdjustment).ToString(), page.Find(".current-price").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_RejectChange_KeepsThePrice()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var request = TestDtos.NewDecisionRequest();
        var order = TestDtos.NewOrder() with { Status = "WaitingForCustomer", CustomerDecisionRequests = [request] };
        var page = RenderOrder(order);

        // Act
        page.Find("button.reject").Click();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains(_orders.Decisions, d => d.Id == order.Id && d.RequestId == request.Id && !d.Approved);
            Assert.Contains(Examples.AgreedPrice.ToString(), page.Find(".current-price").TextContent);
            Assert.Contains("Rejected", page.Find(".decisionrequests").TextContent);
        });
    }

    [Fact]
    public void OrderDetailsPage_ScheduleFitting_ShowsTheAppointment()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        var page = RenderOrder(order);
        var scheduledAt = DateTimeOffset.Parse("2026-11-26T10:00", CultureInfo.InvariantCulture);

        // Act
        page.Find("form.fitting input.scheduledat").Change("2026-11-26T10:00");
        page.Find("form.fitting").Submit();

        // Assert
        page.WaitForAssertion(() =>
        {
            Assert.Contains((order.Id, scheduledAt), _orders.ScheduledFittings);
            Assert.Contains("WaitingForFitting", page.Find(".status").TextContent);
            Assert.Single(page.FindAll("ul.fittings li"));
        });
    }

    // The order, and its customer, as the fake clients give them to the page.
    private IRenderedComponent<OrderDetailsPage> RenderOrder(OrderDto order)
    {
        _orders.Items.Add(order);
        _customers.Items.Add(TestDtos.NewCustomer() with { Id = order.CustomerId });
        return Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));
    }
}
