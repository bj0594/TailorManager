using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Api.Auth;

// Registers users and logs them in.
public class AuthService
{
    private readonly IUserRepository _users;
    private readonly PasswordHasher _hasher;

    public AuthService(IUserRepository users, PasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
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
}
