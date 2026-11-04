using Gruppi.Data;
using Gruppi.Dtos;
using Gruppi.Models;
using UniProjectHub.Shared;

namespace Gruppi.Services;

public class AccessoService(IGruppoRepository repo, INotificaPublisher notifiche) : IAccessoService
{
    public async Task<AccessoDto?> RequestAsync(string studenteEmail, int idGruppo)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null || g.Stato != StatoGruppo.NUOVO) return null;
        if (!await repo.StudenteExistsAsync(studenteEmail)) return null;
        if (await repo.IsMembroAsync(idGruppo, studenteEmail)) return null;
        if (await repo.FindGruppoDiStudenteInSessioneAsync(studenteEmail, g.IdSessione) is not null) return null;

        var richiesta = new Richiesta { Tipo = TipoRichiesta.ACCESSO, Esito = null, CreatedAt = DateTime.UtcNow };
        var accesso = new RichiestaAccesso { IdGruppo = idGruppo, StudenteEmail = studenteEmail };
        var id = await repo.AddAccessoAsync(richiesta, accesso);
        if (g.CapogruppoEmail is not null)
            await notifiche.PublishAsync(g.CapogruppoEmail, "RICHIESTA_ACCESSO",
                $"{studenteEmail} chiede di entrare nel gruppo #{idGruppo}");
        return new AccessoDto(id, idGruppo, studenteEmail, null);
    }

    public async Task<List<AccessoDto>> ListPerGruppoAsync(string capogruppoEmail, int idGruppo)
    {
        var g = await repo.FindAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return new();
        var list = await repo.ListAccessiPerGruppoAsync(idGruppo, soloPendenti: true);
        return list.Select(t => new AccessoDto(t.Item1.IdRichiesta, t.Item2.IdGruppo, t.Item2.StudenteEmail, t.Item1.Esito)).ToList();
    }

    public async Task<string> AcceptAsync(string capogruppoEmail, int idRichiesta)
    {
        var (richiesta, accesso) = await Load(idRichiesta);
        if (richiesta is null || accesso is null) return "Richiesta non trovata";
        var g = await repo.FindAsync(accesso.IdGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return "Non autorizzato";
        if (richiesta.Esito is not null) return "Richiesta già gestita";
        if (g.Stato != StatoGruppo.NUOVO) return "Gruppo non più modificabile";
        if (await repo.CountMembriAsync(g.IdGruppo) >= g.Dimensione) return "Gruppo al completo";
        if (await repo.FindGruppoDiStudenteInSessioneAsync(accesso.StudenteEmail, g.IdSessione) is not null)
            return "Lo studente è già in un gruppo per questa sessione";

        await repo.AddMembroAsync(new MembroGruppo { IdGruppo = g.IdGruppo, StudenteEmail = accesso.StudenteEmail, DataIngresso = DateTime.UtcNow });
        richiesta.Esito = true;
        await repo.UpdateRichiestaAsync(richiesta);
        await notifiche.PublishAsync(accesso.StudenteEmail, "ACCESSO_ESITO",
            $"La tua richiesta di accesso al gruppo #{g.IdGruppo} è stata accettata");
        return "OK";
    }

    public async Task<string> RejectAsync(string capogruppoEmail, int idRichiesta)
    {
        var (richiesta, accesso) = await Load(idRichiesta);
        if (richiesta is null || accesso is null) return "Richiesta non trovata";
        var g = await repo.FindAsync(accesso.IdGruppo);
        if (g is null || g.CapogruppoEmail != capogruppoEmail) return "Non autorizzato";
        if (richiesta.Esito is not null) return "Richiesta già gestita";
        richiesta.Esito = false;
        await repo.UpdateRichiestaAsync(richiesta);
        await notifiche.PublishAsync(accesso.StudenteEmail, "ACCESSO_ESITO",
            $"La tua richiesta di accesso al gruppo #{g.IdGruppo} è stata rifiutata");
        return "OK";
    }

    private async Task<(Richiesta?, RichiestaAccesso?)> Load(int idRichiesta) =>
        (await repo.FindRichiestaAsync(idRichiesta), await repo.FindAccessoAsync(idRichiesta));
}
