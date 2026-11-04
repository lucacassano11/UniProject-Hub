using Sessioni.Data;
using Sessioni.Dtos;
using Sessioni.Models;

namespace Sessioni.Services;

public class SessionService(ISessionRepository repo, ISessionCodeGenerator codes) : ISessionService
{
    public async Task<SessionDto> CreateAsync(string docenteEmail, CreateSessionRequest request)
    {
        string id;
        do { id = codes.Generate(); } while (await repo.ExistsAsync(id));

        var sessione = new Sessione
        {
            IdSessione = id,
            VotoMassimo = request.VotoMassimo,
            DataCreazioneSessione = DateOnly.FromDateTime(DateTime.UtcNow),
            DocenteEmail = docenteEmail,
        };
        await repo.AddAsync(sessione);
        return await ToDto(sessione);
    }

    public async Task<List<SessionDto>> ListAsync(string docenteEmail)
    {
        var list = await repo.ListByDocenteAsync(docenteEmail);
        var dtos = new List<SessionDto>();
        foreach (var s in list) dtos.Add(await ToDto(s));
        return dtos;
    }

    public async Task<SessionDto?> GetAsync(string docenteEmail, string idSessione)
    {
        var s = await repo.FindAsync(idSessione);
        return s is null || s.DocenteEmail != docenteEmail ? null : await ToDto(s);
    }

    public async Task<bool> SetInizioAbstractAsync(string docenteEmail, string idSessione, InizioAbstractRequest request)
    {
        var s = await repo.FindAsync(idSessione);
        if (s is null || s.DocenteEmail != docenteEmail) return false;
        s.InizioInvioAbstract = request.InizioInvioAbstract;
        await repo.UpdateAsync(s);
        return true;
    }

    public async Task<string> DeleteAsync(string docenteEmail, string idSessione)
    {
        var s = await repo.FindAsync(idSessione);
        if (s is null) return "Sessione non trovata";
        if (s.DocenteEmail != docenteEmail) return "Non sei il proprietario della sessione";
        if (await repo.CountGruppiAsync(idSessione) > 0)
            return "Impossibile eliminare: la sessione ha gruppi associati";
        await repo.DeleteAsync(s);
        return "OK";
    }

    private async Task<SessionDto> ToDto(Sessione s)
    {
        var appelli = await repo.ListAppelliAsync(s.IdSessione);
        return new SessionDto(s.IdSessione, s.VotoMassimo, s.DataCreazioneSessione,
            s.InizioInvioAbstract, appelli.Count);
    }
}
