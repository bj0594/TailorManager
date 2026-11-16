namespace TailorManager.Web.Api;

// Holds the logged-in user's token for this browser tab.
public class TokenStore
{
    public string? Token { get; set; }

    public bool IsLoggedIn => Token is not null;
}
