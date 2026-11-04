namespace Auth.Services;

public interface IPasswordPolicy
{
    bool IsValid(string password);
    string Hash(string password);
    bool Verify(string password, string hash);
}
