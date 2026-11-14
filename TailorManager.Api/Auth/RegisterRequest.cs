using System.ComponentModel.DataAnnotations;

namespace TailorManager.Api.Auth;

public record RegisterRequest([Required] string Username, [Required] string Password);
