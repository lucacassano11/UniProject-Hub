using Gruppi.Data;
using Gruppi.Dtos;
using Gruppi.Models;

namespace Gruppi.Services;

public class DimensioneService(IGruppoRepository repo) : IDimensioneService
{
    public async Task<DimensioneDto?> RequestAsync(string capogruppoEmail, int idGruppo, DimensioneRequest request)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return null;
        if (g.Stato != StatoGruppo.NUOVO) return null;

        var testo = $"[nuovaDimensione={request.NuovaDimensione}] {request.Descrizione}";
        var richiesta = new Richiesta { Tipo = TipoRichiesta.DIMENSIONE_GRUPPO, Esito = null, CreatedAt = DateTime.UtcNow };
        var dim = new RichiestaDimensione { IdSessione = g.IdSessione, IdGruppo = idGruppo, Descrizione = testo };
        var id = await repo.AddDimensioneAsync(richiesta, dim);
        return new DimensioneDto(id, idGruppo, g.IdSessione, testo, null);
    }

    public async Task<List<DimensioneDto>> ListPerGruppoAsync(string capogruppoEmail, int idGruppo)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return new();
        var list = await repo.ListDimensioniPerGruppoAsync(idGruppo);
        return list.Select(t => new DimensioneDto(t.Item1.IdRichiesta, t.Item2.IdGruppo, t.Item2.IdSessione, t.Item2.Descrizione, t.Item1.Esito)).ToList();
    }
}
