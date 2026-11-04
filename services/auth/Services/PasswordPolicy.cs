using System.Text.RegularExpressions;

namespace Auth.Services;

public class PasswordPolicy : IPasswordPolicy
{
    public bool IsValid(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
        return Regex.IsMatch(password, "[A-Z]")
            && Regex.IsMatch(password, "[a-z]")
            && Regex.IsMatch(password, "[0-9]")
            && Regex.IsMatch(password, "[^A-Za-z0-9]");
    }

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
