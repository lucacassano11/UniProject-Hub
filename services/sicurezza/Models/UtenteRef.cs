namespace Sicurezza.Models;

public class UtenteRef
{
    public string Email { get; set; } = "";
    public string Ruolo { get; set; } = "";
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
}
