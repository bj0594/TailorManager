using Microsoft.AspNetCore.Mvc;
using TailorManager.Core;
using Microsoft.AspNetCore.Authorization;
using TailorManager.Core.Repository;

namespace TailorManager.Api.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    private readonly IUserRepository _userRepository;

    public OrdersController(OrderService service, IUserRepository userRepository)
    {
        _service = service;
        _userRepository = userRepository;
    }

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost]
    public IActionResult Create(CreateOrderRequest request)
    {
        var order = _service.AddOrder(request.CustomerId, request.Type, request.WorkDescription, request.AgreedPrice, request.RelatedOrderId, CurrentEmployee());
        return Created($"/orders/{order.Id}", order);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(_service.GetOrder(id));
    }

    [HttpPatch("{id}/start")]
    public IActionResult Start(Guid id)
    {
        _service.StartOrder(id);
        return NoContent();
    }

    // The logged-in employee, from the username in the token. Every action that writes the history sends it to the service.
    private User CurrentEmployee() => _userRepository.GetByUsername(User.Identity!.Name!)!;
}
