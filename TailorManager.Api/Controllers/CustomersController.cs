using Microsoft.AspNetCore.Mvc;
using TailorManager.Core;

namespace TailorManager.Api.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service) => _service = service;

    [HttpPost]
    public IActionResult Create(CreateCustomerRequest request)
    {
        var customer = _service.AddCustomer(request.Name, request.Email, request.PhoneNumber, request.Address);
        return Created($"/customers/{customer.Id}", customer);
    }
}
