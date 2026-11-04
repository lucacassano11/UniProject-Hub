using Sessioni.Data;

namespace Sessioni.Services;

public class SessionCleanup(ISessionRepository repo) : ISessionCleanup
{
    public async Task<int> RunOnceAsync(DateOnly now)
    {
        var soglia = now.AddMonths(-6);
        var candidate = await repo.ListCreatedBeforeAsync(soglia);
        var deleted = 0;
        foreach (var s in candidate)
        {
            if (await repo.CountGruppiAsync(s.IdSessione) == 0)
            {
                // elimina appelli collegati prima della sessione
                foreach (var a in await repo.ListAppelliAsync(s.IdSessione))
                    await repo.DeleteAppelloAsync(a);
                await repo.DeleteAsync(s);
                deleted++;
            }
        }
        return deleted;
    }
}
