using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorManager.Api.Auth;

namespace TailorManager.Api.Controllers;

// Register, log in and /auth/me (api.md). The errors from AuthService become 409, 400 and 401 in DomainExceptionFilter.
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        var user = _authService.Register(request.Username, request.Password, request.FirstName, request.LastName, request.Role);
        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.Username });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request) => Ok(new { token = _authService.Login(request.Username, request.Password) });

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(User.Claims.Select(c => new { c.Type, c.Value }));
}
