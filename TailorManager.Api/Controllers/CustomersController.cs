using Microsoft.AspNetCore.Mvc;
using TailorManager.Core;
using Microsoft.AspNetCore.Authorization;

namespace TailorManager.Api.Controllers;

[ApiController]
[Route("customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service) => _service = service;

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost]
    public IActionResult Create(CreateCustomerRequest request)
    {
        var customer = _service.AddCustomer(request.Name, request.Phone, request.Email, request.Address);
        return Created($"/customers/{customer.Id}", customer);
    }

    [HttpGet("{customerId}")]
    public IActionResult GetById(Guid customerId)
    {
        return Ok(_service.GetCustomer(customerId));
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? searchTerm)
    {
        return Ok(_service.FindCustomers(searchTerm!));
    }
}
