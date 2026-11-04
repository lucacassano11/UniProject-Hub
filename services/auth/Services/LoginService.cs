using Auth.Data;
using Auth.Dtos;
using UniProjectHub.Shared;

namespace Auth.Services;

public class LoginService(IUserRepository repo, IPasswordPolicy passwords, IJwtService jwt, IEventoLogger log)
    : ILoginService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var utente = await repo.FindByEmailAsync(email);
        if (utente is null)
            return new(false, "Credenziali non valide");

        if (utente.LockedUntil is { } until && until > DateTime.UtcNow)
            return new(false, "Account bloccato, riprova piu tardi");

        if (!passwords.Verify(request.Password, utente.PasswordHash))
        {
            utente.FailedAttempts++;
            await log.LogAsync("LOGIN_FALLITO", email, $"Tentativo di accesso fallito ({utente.FailedAttempts})");
            if (utente.FailedAttempts >= MaxAttempts)
            {
                utente.LockedUntil = DateTime.UtcNow.Add(LockDuration);
                await log.LogAsync("ACCOUNT_BLOCCATO", email, "Account bloccato automaticamente dopo troppi tentativi");
            }
            await repo.UpdateAsync(utente);
            return new(false, "Credenziali non valide");
        }

        utente.FailedAttempts = 0;
        utente.LockedUntil = null;
        await repo.UpdateAsync(utente);

        var token = jwt.GenerateToken(utente.Email, utente.Ruolo);
        return new(true, User: new AuthResponse(utente.Email, utente.Ruolo.ToString(), token));
    }
}
