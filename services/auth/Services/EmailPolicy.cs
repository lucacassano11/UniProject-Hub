using Auth.Models;

namespace Auth.Services;

public class EmailPolicy : IEmailPolicy
{
    private const string StudentDomain = "@studio.unibo.it";
    private const string TeacherDomain = "@unibo.it";

    public bool IsUniversityEmail(string email) =>
        email.EndsWith(StudentDomain, StringComparison.OrdinalIgnoreCase)
        || email.EndsWith(TeacherDomain, StringComparison.OrdinalIgnoreCase);

    public Ruolo RoleFromEmail(string email) =>
        email.EndsWith(StudentDomain, StringComparison.OrdinalIgnoreCase)
            ? Ruolo.STUDENTE
            : Ruolo.DOCENTE;

    public (string Nome, string Cognome) DeriveName(string email)
    {
        var local = email.Split('@')[0];
        var parts = local.Split('.', 2);
        string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
        var nome = Cap(parts[0]);
        var cognome = parts.Length > 1 ? Cap(parts[1]) : "";
        return (nome, cognome);
    }
}
