using Microsoft.EntityFrameworkCore;
using Auth.Data;
using UniProjectHub.Shared;

namespace Auth.Logging;

/// <summary>Scrive eventi nel log centralizzato `log_eventi` (best-effort).</summary>
public class EfEventoLogger(AuthDbContext db) : IEventoLogger
{
    public async Task LogAsync(string tipo, string? attoreEmail, string dettaglio)
    {
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO log_eventi(tipo,attore_email,dettaglio) VALUES({tipo},{attoreEmail},{dettaglio})");
        }
        catch
        {
            // best-effort
        }
    }
}
