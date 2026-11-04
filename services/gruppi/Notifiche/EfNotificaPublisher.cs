using Microsoft.EntityFrameworkCore;
using Gruppi.Data;
using UniProjectHub.Shared;

namespace Gruppi.Notifiche;

/// <summary>Pubblica notifiche inserendo nella tabella condivisa `notifiche` (best-effort).</summary>
public class EfNotificaPublisher(GruppiDbContext db) : INotificaPublisher
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
