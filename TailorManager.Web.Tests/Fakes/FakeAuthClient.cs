using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers what the test asks for: Succeeds = true or false, and Error is the message a failed registration gives.
public class FakeAuthClient : IAuthClient
{
    public bool Succeeds { get; set; }
    public string Error { get; set; } = "Username is already taken.";

    public Task<bool> LoginAsync(string username, string password) => Task.FromResult(Succeeds);
    public Task<bool> RegisterAsync(string username, string password, string firstName, string lastName, string role) => Task.FromResult(Succeeds);
}
