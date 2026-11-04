using Microsoft.EntityFrameworkCore;
using Consegne.Data;
using UniProjectHub.Shared;

namespace Consegne.Notifiche;

/// <summary>Pubblica notifiche inserendo nella tabella condivisa `notifiche` (best-effort).</summary>
public class EfNotificaPublisher(ConsegneDbContext db) : INotificaPublisher
{
    public async Task PublishAsync(string destinatarioEmail, string tipo, string messaggio)
    {
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO notifiche(destinatario_email,tipo,messaggio) VALUES({destinatarioEmail},{tipo},{messaggio})");
        }
        catch
        {
            // best-effort: un errore nella notifica non deve far fallire l'azione di dominio
        }
    }
}
