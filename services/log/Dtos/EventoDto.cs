namespace Log.Dtos;

public record EventoDto(int IdEvento, string Tipo, string? AttoreEmail, string Dettaglio, DateTime CreatedAt);
