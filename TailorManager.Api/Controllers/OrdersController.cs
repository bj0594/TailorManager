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
        var order = _service.AddOrder(
            request.CustomerId,
            request.Type,
            request.WorkDescription,
            request.AgreedPrice,
            request.RelatedOrderId,
            CurrentEmployee());

        return Created($"/orders/{order.Id}", order);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(_service.GetOrder(id));
    }

    [Authorize(Roles = Roles.Workshop)]
    [HttpPatch("{id}/start")]
    public IActionResult Start(Guid id)
    {
        try
        {
            return Ok(_service.StartOrder(id, CurrentEmployee()));
        }
        catch (InvalidOrderStatusException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [Authorize(Roles = Roles.Workshop)]
    [HttpPost("{id}/notes")]
    public IActionResult AddWorkNote(Guid id, AddWorkNoteRequest request)
    {
        return Ok(_service.AddWorkNote(id, request.Note, CurrentEmployee()));
    }

    [Authorize(Roles = Roles.Workshop)]
    [HttpPatch("{id}/ready-for-pickup")]
    public IActionResult MarkReadyForPickup(Guid id)
    {
        return Ok(_service.MarkReadyForPickup(id, CurrentEmployee()));
    }

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id}/payments")]
    public IActionResult AddPayment(Guid id, AddPaymentRequest request)
    {
        return Ok(_service.AddPayment(id, request.Amount));
    }

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPatch("{id}/deliver")]
    public IActionResult Deliver(Guid id)
    {
        _service.DeliverOrder(id);
        return NoContent();
    }

    // The logged-in employee, from the username in the token. Every action that writes the history sends it to the service.
    private User CurrentEmployee() => _userRepository.GetByUsername(User.Identity!.Name!)!;
    public record AddWorkNoteRequest(string Note);
}
