using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Api.Auth;

// Registers users and logs them in.
public class AuthService
{
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users)
    {
        _users = users;
    }

    public User Register(string username, string password)
    {
        var user = new User { Id = Guid.NewGuid(), Username = username, PasswordHash = password };
        _users.Add(user);
        return user;
    }
}
