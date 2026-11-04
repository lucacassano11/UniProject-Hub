namespace Sicurezza.Dtos;

public record UtenteSicurezzaDto(string Email, string Ruolo, int FailedAttempts, DateTime? LockedUntil, bool Bloccato);
