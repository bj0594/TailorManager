using Microsoft.AspNetCore.Mvc;
using TailorManager.Core;

namespace TailorManager.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service) => _service = service;

    [HttpPost]
    public IActionResult Create(CreateOrderRequest request)
    {
        var order = _service.AddOrder(request.CustomerId, request.Type, request.WorkDescription, request.AgreedPrice, request.RelatedOrderId);
        return Created($"/orders/{order.Id}", order);
    }
}
