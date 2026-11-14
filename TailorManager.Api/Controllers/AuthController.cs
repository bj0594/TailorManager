using Microsoft.AspNetCore.Mvc;
using TailorManager.Api.Auth;

namespace TailorManager.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        var user = _authService.Register(request.Username, request.Password);
        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.Username });
    }
}
