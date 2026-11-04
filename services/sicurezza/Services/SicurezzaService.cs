using Sicurezza.Data;
using Sicurezza.Dtos;
using UniProjectHub.Shared;

namespace Sicurezza.Services;

public class SicurezzaService(ISicurezzaRepository repo, IEventoLogger log) : ISicurezzaService
{
    // locked_until è un TIMESTAMP MySQL (max ~2038): usiamo una data lontana entro il range come "blocco indeterminato".
    private static readonly DateTime BloccoUntil = new(2037, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public async Task<List<UtenteSicurezzaDto>> ListUtentiAsync()
    {
        var now = DateTime.UtcNow;
        var list = await repo.ListUtentiAsync();
        return list.Select(u => new UtenteSicurezzaDto(
            u.Email, u.Ruolo, u.FailedAttempts, u.LockedUntil,
            u.LockedUntil is { } until && until > now)).ToList();
    }

    public async Task<bool> BloccaAsync(string gestoreEmail, string targetEmail)
    {
        var u = await repo.FindUtenteAsync(targetEmail);
        if (u is null) return false;
        u.LockedUntil = BloccoUntil;
        await repo.UpdateUtenteAsync(u);
        await log.LogAsync("BLOCCO_MANUALE", gestoreEmail, $"Account {targetEmail} bloccato manualmente");
        return true;
    }

    public async Task<bool> SbloccaAsync(string gestoreEmail, string targetEmail)
    {
        var u = await repo.FindUtenteAsync(targetEmail);
        if (u is null) return false;
        u.FailedAttempts = 0;
        u.LockedUntil = null;
        await repo.UpdateUtenteAsync(u);
        await log.LogAsync("SBLOCCO_MANUALE", gestoreEmail, $"Account {targetEmail} sbloccato");
        return true;
    }
}
