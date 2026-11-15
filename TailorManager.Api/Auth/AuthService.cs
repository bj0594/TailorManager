using TailorManager.Core;
using TailorManager.Core.Repository;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace TailorManager.Api.Auth;

// Registers users and logs them in.
public class AuthService
{
    private readonly IUserRepository _users;
    private readonly PasswordHasher _hasher;
    private readonly IConfiguration _config;
    public AuthService(IUserRepository users, PasswordHasher hasher, IConfiguration config)
    {
        _users = users;
        _hasher = hasher;
        _config = config;
    }

    public User Register(string username, string password)
    {
        if (_users.GetByUsername(username) is not null)
        {
            throw new InvalidOperationException("Username is already taken.");
        }
        var user = new User { Id = Guid.NewGuid(), Username = username, PasswordHash = _hasher.Hash(password) };
        _users.Add(user);
        return user;
    }

    // Returns a JWT. Throws UnauthorizedAccessException if the username or the password is wrong.
    public string Login(string username, string password)
    {
        var user = _users.GetByUsername(username);
        if (user is null || !_hasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Wrong username or password.");
        }

        return CreateToken(user);
    }

    // Creates the JWT a user gets after logging in. Uses the same Jwt settings as the check in Program.cs.
    private string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
        }),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
