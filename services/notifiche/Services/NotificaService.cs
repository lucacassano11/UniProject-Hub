using Notifiche.Data;
using Notifiche.Dtos;

namespace Notifiche.Services;

public class NotificaService(INotificheRepository repo) : INotificaService
{
    public async Task<List<NotificaDto>> ListAsync(string email, bool soloNonLette)
    {
        var list = await repo.ListAsync(email, soloNonLette);
        return list.Select(n => new NotificaDto(n.IdNotifica, n.Tipo, n.Messaggio, n.Letta, n.CreatedAt)).ToList();
    }

    public Task<int> CountNonLetteAsync(string email) => repo.CountNonLetteAsync(email);

    public async Task<bool> MarkLettaAsync(string email, int idNotifica)
    {
        var n = await repo.FindAsync(idNotifica);
        if (n is null || n.DestinatarioEmail != email) return false;
        await repo.MarkLettaAsync(n);
        return true;
    }
}
