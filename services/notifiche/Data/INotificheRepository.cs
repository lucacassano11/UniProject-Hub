using Notifiche.Models;

namespace Notifiche.Data;

public interface INotificheRepository
{
    Task<List<Notifica>> ListAsync(string email, bool soloNonLette);
    Task<int> CountNonLetteAsync(string email);
    Task<Notifica?> FindAsync(int idNotifica);
    Task MarkLettaAsync(Notifica n);
}
