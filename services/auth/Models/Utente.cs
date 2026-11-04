namespace Auth.Models;

public class Utente
{
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cognome { get; set; } = "";
    public Ruolo Ruolo { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
}
