using Auth.Dtos;

namespace Auth.Services;

public record LoginResult(bool Success, string Error = "", AuthResponse? User = null);

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
}
