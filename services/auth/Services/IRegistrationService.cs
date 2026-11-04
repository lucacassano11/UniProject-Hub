using Auth.Dtos;

namespace Auth.Services;

public record RegistrationResult(bool Success, string Error = "", AuthResponse? User = null);

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);
}
