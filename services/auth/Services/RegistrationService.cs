using Auth.Data;
using Auth.Dtos;
using Auth.Models;

namespace Auth.Services;

public class RegistrationService(IUserRepository repo, IPasswordPolicy passwords, IEmailPolicy emails)
    : IRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (!emails.IsUniversityEmail(email))
            return new(false, "L'email non e universitaria");

        if (!Enum.TryParse<Ruolo>(request.Ruolo, out var ruolo)
            || ruolo == Ruolo.GESTORE_SICUREZZA)
            return new(false, "Ruolo non valido");

        if (emails.RoleFromEmail(email) != ruolo)
            return new(false, "Il ruolo non corrisponde al dominio dell'email");

        if (!passwords.IsValid(request.Password))
            return new(false, "La password non rispetta i requisiti di complessita");

        if (await repo.ExistsAsync(email))
            return new(false, "Email gia utilizzata");

        var (nome, cognome) = emails.DeriveName(email);
        var utente = new Utente
        {
            Email = email,
            PasswordHash = passwords.Hash(request.Password),
            Nome = nome,
            Cognome = cognome,
            Ruolo = ruolo,
            CreatedAt = DateTime.UtcNow,
        };

        if (ruolo == Ruolo.STUDENTE)
            await repo.AddStudenteAsync(utente, await GenerateMatricolaAsync());
        else
            await repo.AddDocenteAsync(utente);

        return new(true, User: new AuthResponse(email, ruolo.ToString()));
    }

    private async Task<string> GenerateMatricolaAsync()
    {
        string m;
        do { m = "S" + Random.Shared.Next(0, 1_000_000).ToString("D6"); }
        while (await repo.MatricolaExistsAsync(m));
        return m;
    }
}
