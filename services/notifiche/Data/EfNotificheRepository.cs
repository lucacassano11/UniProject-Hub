using Microsoft.EntityFrameworkCore;
using Notifiche.Models;

namespace Notifiche.Data;

public class EfNotificheRepository(NotificheDbContext db) : INotificheRepository
{
    public Task<List<Notifica>> ListAsync(string email, bool soloNonLette) =>
        db.Notifiche
            .Where(n => n.DestinatarioEmail == email && (!soloNonLette || !n.Letta))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public Task<int> CountNonLetteAsync(string email) =>
        db.Notifiche.CountAsync(n => n.DestinatarioEmail == email && !n.Letta);

    public Task<Notifica?> FindAsync(int idNotifica) =>
        db.Notifiche.FirstOrDefaultAsync(n => n.IdNotifica == idNotifica);

    public async Task MarkLettaAsync(Notifica n)
    {
        n.Letta = true;
        db.Notifiche.Update(n);
        await db.SaveChangesAsync();
    }
}
