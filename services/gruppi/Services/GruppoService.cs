using Gruppi.Data;
using Gruppi.Dtos;
using Gruppi.Models;

namespace Gruppi.Services;

public class GruppoService(IGruppoRepository repo) : IGruppoService
{
    public async Task<GruppoDto> CreateAsync(string studenteEmail, CreateGruppoRequest request)
    {
        if (!await repo.SessioneExistsAsync(request.IdSessione))
            throw new InvalidOperationException("Sessione inesistente");
        if (await repo.FindGruppoDiStudenteInSessioneAsync(studenteEmail, request.IdSessione) is not null)
            throw new InvalidOperationException("Sei già in un gruppo per questa sessione");

        var gruppo = new Gruppo
        {
            IdSessione = request.IdSessione,
            Dimensione = 3,
            Stato = StatoGruppo.NUOVO,
            CapogruppoEmail = studenteEmail,
        };
        await repo.AddAsync(gruppo);
        await repo.AddMembroAsync(new MembroGruppo
        {
            IdGruppo = gruppo.IdGruppo, StudenteEmail = studenteEmail, DataIngresso = DateTime.UtcNow,
        });
        return await ToDto(gruppo);
    }

    public async Task<List<GruppoDto>> ListMieiAsync(string studenteEmail)
    {
        var membership = await repo.ListGruppiDiStudenteAsync(studenteEmail);
        var dtos = new List<GruppoDto>();
        foreach (var m in membership)
            if (await repo.FindAsync(m.IdGruppo) is { } g) dtos.Add(await ToDto(g));
        return dtos;
    }

    public async Task<List<GruppoDto>> ListBySessioneAsync(string idSessione)
    {
        var list = await repo.ListBySessioneAsync(idSessione);
        var dtos = new List<GruppoDto>();
        foreach (var g in list) dtos.Add(await ToDto(g));
        return dtos;
    }

    public async Task<GruppoDto?> GetAsync(int idGruppo)
    {
        var g = await repo.FindAsync(idGruppo);
        return g is null ? null : await ToDto(g);
    }

    public Task<bool> SessioneEsisteAsync(string idSessione) => repo.SessioneExistsAsync(idSessione);

    public async Task<string> LeaveAsync(string studenteEmail, int idGruppo)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null) return "Gruppo non trovato";
        if (g.Stato != StatoGruppo.NUOVO) return "Gruppo non più modificabile";
        if (!await repo.IsMembroAsync(idGruppo, studenteEmail)) return "Non sei membro del gruppo";

        await repo.RemoveMembroAsync(new MembroGruppo { IdGruppo = idGruppo, StudenteEmail = studenteEmail });
        var rimasti = await repo.ListMembriAsync(idGruppo);
        if (rimasti.Count == 0)
        {
            await repo.DeleteRichiesteDiGruppoAsync(idGruppo);
            await repo.DeleteAsync(g);
        }
        else if (g.CapogruppoEmail == studenteEmail)
        {
            g.CapogruppoEmail = rimasti[0].StudenteEmail; // più anziano (ListMembri ordina per data_ingresso)
            await repo.UpdateAsync(g);
        }
        return "OK";
    }

    private async Task<GruppoDto> ToDto(Gruppo g)
    {
        var membri = await repo.ListMembriAsync(g.IdGruppo);
        var membriDto = membri.Select(m => new MembroDto(m.StudenteEmail, m.StudenteEmail == g.CapogruppoEmail)).ToList();
        return new GruppoDto(g.IdGruppo, g.IdSessione, g.Dimensione, g.Stato.ToString(),
            g.CapogruppoEmail, membri.Count, membriDto);
    }
}
