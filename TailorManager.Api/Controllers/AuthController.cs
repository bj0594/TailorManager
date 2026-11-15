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
        try
        {
            var user = _authService.Register(request.Username, request.Password);
            return StatusCode(StatusCodes.Status201Created, new { user.Id, user.Username });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        try
        {
            var token = _authService.Login(request.Username, request.Password);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}
