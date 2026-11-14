namespace TailorManager.Core;

// A user who can log in. Only the hash of the password is stored.
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
