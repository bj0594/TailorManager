namespace TailorManager.Core.Repository;

public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void Add(User user) => _users.Add(user);
    public User? GetByUsername(string username) => _users.FirstOrDefault(u => u.Username == username);
}
