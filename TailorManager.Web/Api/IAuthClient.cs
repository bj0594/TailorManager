namespace TailorManager.Web.Api;

// Logs in and registers through the API. AuthClient calls the API, and FakeAuthClient is used in the tests.
public interface IAuthClient
{
    Task<bool> LoginAsync(string username, string password);

    // Returns null when the user is created, or the message to show when the API says no.
    Task<string?> RegisterAsync(string username, string password, string firstName, string lastName, string role);
}
