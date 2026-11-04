using Sessioni.Data;
using Sessioni.Dtos;
using Sessioni.Models;

namespace Sessioni.Services;

public class AppelloService(ISessionRepository repo) : IAppelloService
{
    public async Task<List<AppelloDto>> ListAsync(string docenteEmail, string idSessione)
    {
        var s = await repo.FindAsync(idSessione);
        if (s is null || s.DocenteEmail != docenteEmail) return new();
        var appelli = await repo.ListAppelliAsync(idSessione);
        return appelli.Select(a => new AppelloDto(a.IdAppello, a.DataOraAppello, a.ScadenzaConsegnaProgetto)).ToList();
    }

    public async Task<AppelloDto?> AddAsync(string docenteEmail, string idSessione, CreateAppelloRequest request)
    {
        var s = await repo.FindAsync(idSessione);
        if (s is null || s.DocenteEmail != docenteEmail) return null;
        var appello = new Appello
        {
            IdSessione = idSessione,
            DataOraAppello = request.DataOraAppello,
            ScadenzaConsegnaProgetto = request.ScadenzaConsegnaProgetto,
        };
        await repo.AddAppelloAsync(appello);
        return new AppelloDto(appello.IdAppello, appello.DataOraAppello, appello.ScadenzaConsegnaProgetto);
    }

    public async Task<string> DeleteAsync(string docenteEmail, int idAppello)
    {
        var a = await repo.FindAppelloAsync(idAppello);
        if (a is null) return "Appello non trovato";
        var s = await repo.FindAsync(a.IdSessione);
        if (s is null || s.DocenteEmail != docenteEmail) return "Non autorizzato";
        await repo.DeleteAppelloAsync(a);
        return "OK";
    }
}
