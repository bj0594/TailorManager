namespace TailorManager.Api.Auth;

// Hashes passwords with BCrypt. The password itself is never stored.
public class PasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
