using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Api.Controllers;

// The order endpoints in api.md. Every action answers 200 OK with the order.
// The errors from Core become 404, 409 and 400 in DomainExceptionFilter.
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

    [HttpGet("{orderId}")]
    public IActionResult GetById(Guid orderId) => Ok(_service.GetOrder(orderId));

    [Authorize(Roles = Roles.Workshop)]
    [HttpPatch("{orderId}/start")]
    public IActionResult StartWork(Guid orderId) => Ok(_service.StartOrder(orderId, CurrentEmployee()));

    [Authorize(Roles = Roles.Workshop)]
    [HttpPost("{orderId}/notes")]
    public IActionResult AddWorkNote(Guid orderId, AddWorkNoteRequest request) =>
        Ok(_service.AddWorkNote(orderId, request.Note, CurrentEmployee()));

    [Authorize(Roles = Roles.Workshop)]
    [HttpPatch("{orderId}/ready-for-pickup")]
    public IActionResult MarkReadyForPickup(Guid orderId) => Ok(_service.MarkReadyForPickup(orderId, CurrentEmployee()));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{orderId}/payments")]
    public IActionResult RegisterPayment(Guid orderId, RegisterPaymentRequest request) =>
        Ok(_service.RegisterPayment(orderId, request.Amount, request.TransactionReference, CurrentEmployee()));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPatch("{orderId}/delivered")]
    public IActionResult MarkDelivered(Guid orderId) => Ok(_service.MarkDelivered(orderId, CurrentEmployee()));

    [HttpGet]
    public IActionResult GetAll([FromQuery] OrderStatus? status, [FromQuery] Guid? customerId)
    {
        return Ok(_service.ListOrders(status, customerId));
    }

    // The logged-in employee, from the username in the token. Every action that writes the history sends it to the service.
    private User CurrentEmployee() => _userRepository.GetByUsername(User.Identity!.Name!)!;
}
