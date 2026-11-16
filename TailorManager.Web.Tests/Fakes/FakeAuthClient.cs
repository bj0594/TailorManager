using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers what the test asks for: Succeeds = true or false.
public class FakeAuthClient : IAuthClient
{
    public bool Succeeds { get; set; }

    public Task<bool> LoginAsync(string username, string password) => Task.FromResult(Succeeds);
    public Task<bool> RegisterAsync(string username, string password, string role) => Task.FromResult(Succeeds);
}
