namespace TailorManager.Core;

// A user who can log in. Only the hash of the password is stored.
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = Roles.Workshop;

    // The name the history shows, like "Kari N.". Only the first name when there is no last name.
    public string DisplayName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName[0]}.";
}
