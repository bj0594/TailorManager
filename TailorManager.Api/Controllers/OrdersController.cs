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

    [Authorize(Roles = "FrontDesk")]
    [HttpPost]
    public IActionResult Create(CreateOrderRequest request)
    {
        var employee = _userRepository.GetByUsername(User.Identity!.Name!)!;
        var order = _service.AddOrder(request.CustomerId, request.Type, request.WorkDescription, request.AgreedPrice, request.RelatedOrderId, employee); 
        return Created($"/orders/{order.Id}", order);
    }
}
