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

    [Authorize(Roles = "FrontDesk")]
    [HttpPost]
    public IActionResult Create(CreateCustomerRequest request)
    {
        var customer = _service.AddCustomer(request.Name, request.Email, request.PhoneNumber, request.Address);
        return Created($"/customers/{customer.Id}", customer);
    }
}
