using System.ComponentModel.DataAnnotations;

namespace TailorManager.Api.Auth;

public record LoginRequest([Required] string Username, [Required] string Password);
