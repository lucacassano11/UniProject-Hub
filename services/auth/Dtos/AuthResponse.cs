namespace Auth.Dtos;

public record AuthResponse(string Email, string Ruolo, string? Token = null);
