namespace Notifiche.Dtos;

public record NotificaDto(int IdNotifica, string Tipo, string Messaggio, bool Letta, DateTime CreatedAt);
