using System.Text.Json;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// The DTOs with the example values from §4, as the API would send them.
// Change one value with `with`: TestDtos.NewOrder() with { Status = "InProgress" }.
public static class TestDtos
{
    public static CustomerDto NewCustomer(string name = Examples.CustomerName) =>
        new(Guid.NewGuid(), name, Examples.CustomerPhone, Examples.CustomerEmail, Examples.CustomerAddress);

    // A new order has the agreed price as the current price and as the balance, and the status Submitted.
    public static OrderDto NewOrder(Guid? customerId = null, string workDescription = Examples.WorkDescription) =>
        new(Guid.NewGuid(), customerId ?? Guid.NewGuid(), "NewGarment", workDescription,
            Examples.AgreedPrice, Examples.AgreedPrice, Examples.AgreedPrice, "Submitted", DateTimeOffset.Now, null);

    // A pending request for the change and the price adjustment in §4, for an order that is WaitingForCustomer.
    public static CustomerDecisionRequestDto NewDecisionRequest(decimal? proposedPriceAdjustment = Examples.ProposedPriceAdjustment) =>
        new(Guid.NewGuid(), Examples.ProposedWorkChange, proposedPriceAdjustment, "Pending");

    // A fitting two days from now that is not completed, for an order that is WaitingForFitting.
    public static FittingDto NewFitting() => new(Guid.NewGuid(), DateTimeOffset.Now.AddDays(2), null, null);

    // The JSON the API sends for an object, with camelCase names, for _handler.Respond in the client tests.
    public static string ToJson(object value) => JsonSerializer.Serialize(value, JsonSerializerOptions.Web);
}
