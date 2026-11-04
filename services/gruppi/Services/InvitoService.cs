using Gruppi.Data;
using Gruppi.Dtos;
using Gruppi.Models;
using UniProjectHub.Shared;

namespace Gruppi.Services;

public class InvitoService(IGruppoRepository repo, INotificaPublisher notifiche) : IInvitoService
{
    public async Task<InvitoDto?> InviteAsync(string capogruppoEmail, int idGruppo, InvitoRequest request)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return null;
        if (g.Stato != StatoGruppo.NUOVO) return null;
        if (!await repo.StudenteExistsAsync(request.StudenteEmail)) return null;
        if (await repo.IsMembroAsync(idGruppo, request.StudenteEmail)) return null;

        var richiesta = new Richiesta { Tipo = TipoRichiesta.INVITO, Esito = null, CreatedAt = DateTime.UtcNow };
        var invito = new Invito { IdGruppo = idGruppo, StudenteEmail = request.StudenteEmail };
        var id = await repo.AddInvitoAsync(richiesta, invito);
        await notifiche.PublishAsync(request.StudenteEmail, "INVITO", $"Sei stato invitato al gruppo #{idGruppo}");
        return new InvitoDto(id, idGruppo, request.StudenteEmail, null);
    }

    public async Task<List<InvitoDto>> ListMieiAsync(string studenteEmail)
    {
        var list = await repo.ListInvitiPerStudenteAsync(studenteEmail, soloPendenti: true);
        return list.Select(t => new InvitoDto(t.Item1.IdRichiesta, t.Item2.IdGruppo, t.Item2.StudenteEmail, t.Item1.Esito)).ToList();
    }

    public async Task<string> AcceptAsync(string studenteEmail, int idRichiesta)
    {
        var (richiesta, invito) = await Load(idRichiesta);
        if (richiesta is null || invito is null) return "Invito non trovato";
        if (invito.StudenteEmail != studenteEmail) return "Invito non destinato a te";
        if (richiesta.Esito is not null) return "Invito già gestito";

        var g = await repo.FindAsync(invito.IdGruppo);
        if (g is null) return "Gruppo non trovato";
        if (g.Stato != StatoGruppo.NUOVO) return "Gruppo non più modificabile";
        if (await repo.CountMembriAsync(g.IdGruppo) >= g.Dimensione) return "Gruppo al completo";
        if (await repo.FindGruppoDiStudenteInSessioneAsync(studenteEmail, g.IdSessione) is not null)
            return "Sei già in un gruppo per questa sessione";

        await repo.AddMembroAsync(new MembroGruppo { IdGruppo = g.IdGruppo, StudenteEmail = studenteEmail, DataIngresso = DateTime.UtcNow });
        richiesta.Esito = true;
        await repo.UpdateRichiestaAsync(richiesta);
        return "OK";
    }

    public async Task<string> RejectAsync(string studenteEmail, int idRichiesta)
    {
        var (richiesta, invito) = await Load(idRichiesta);
        if (richiesta is null || invito is null) return "Invito non trovato";
        if (invito.StudenteEmail != studenteEmail) return "Invito non destinato a te";
        if (richiesta.Esito is not null) return "Invito già gestito";
        richiesta.Esito = false;
        await repo.UpdateRichiestaAsync(richiesta);
        return "OK";
    }

    private async Task<(Richiesta?, Invito?)> Load(int idRichiesta) =>
        (await repo.FindRichiestaAsync(idRichiesta), await repo.FindInvitoAsync(idRichiesta));
}
