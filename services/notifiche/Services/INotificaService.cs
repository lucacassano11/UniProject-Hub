using Notifiche.Dtos;

namespace Notifiche.Services;

public interface INotificaService
{
    Task<List<NotificaDto>> ListAsync(string email, bool soloNonLette);
    Task<int> CountNonLetteAsync(string email);
    Task<bool> MarkLettaAsync(string email, int idNotifica);
}
