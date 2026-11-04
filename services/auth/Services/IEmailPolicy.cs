using Auth.Models;

namespace Auth.Services;

public interface IEmailPolicy
{
    bool IsUniversityEmail(string email);
    Ruolo RoleFromEmail(string email);
    (string Nome, string Cognome) DeriveName(string email);
}
