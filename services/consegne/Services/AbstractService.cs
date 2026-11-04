using Consegne.Data;
using Consegne.Dtos;
using Consegne.Models;
using UniProjectHub.Shared;

namespace Consegne.Services;

public class AbstractService(IConsegnaRepository repo, INotificaPublisher notifiche) : IAbstractService
{
    public async Task<AbstractDto?> SubmitAsync(string studenteEmail, int idGruppo, SubmitAbstractRequest request)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != studenteEmail) return null;
        // consentito se NUOVO, oppure ABSTRACT_CONSEGNATO non ancora approvato (riconsegna)
        var existing = await repo.FindAbstractByGruppoAsync(idGruppo);
        if (g.Stato == StatoGruppo.NUOVO)
        {
            // prima consegna
        }
        else if (g.Stato == StatoGruppo.ABSTRACT_CONSEGNATO && existing is { Esito: not true })
        {
            // riconsegna dopo rifiuto / in attesa
        }
        else
        {
            return null;
        }

        if (existing is null)
        {
            existing = new Abstract
            {
                IdGruppo = idGruppo,
                Descrizione = request.Descrizione,
                DataOraConsegna = DateTime.UtcNow,
                Esito = null,
                ModificheProposte = null,
            };
            await repo.AddAbstractAsync(existing);
        }
        else
        {
            existing.Descrizione = request.Descrizione;
            existing.DataOraConsegna = DateTime.UtcNow;
            existing.Esito = null;
            existing.ModificheProposte = null;
            await repo.UpdateAbstractAsync(existing);
        }

        g.Stato = StatoGruppo.ABSTRACT_CONSEGNATO;
        await repo.UpdateGruppoStatoAsync(g);
        return ToDto(existing);
    }

    public async Task<AbstractDto?> GetAsync(string email, int idGruppo)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null) return null;
        var autorizzato = await repo.IsMembroAsync(idGruppo, email)
            || email == await repo.DocenteDellaSessioneAsync(g.IdSessione);
        if (!autorizzato) return null;
        var a = await repo.FindAbstractByGruppoAsync(idGruppo);
        return a is null ? null : ToDto(a);
    }

    public async Task<string> ApproveAsync(string docenteEmail, int idGruppo)
    {
        var (g, a, err) = await LoadForDocente(docenteEmail, idGruppo);
        if (err is not null) return err;
        if (a!.Esito is not null) return "Abstract già valutato";
        a.Esito = true;
        a.DocenteEmail = docenteEmail;
        await repo.UpdateAbstractAsync(a);
        g!.Stato = StatoGruppo.ABSTRACT_VALUTATO;
        await repo.UpdateGruppoStatoAsync(g);
        if (g.CapogruppoEmail is not null)
            await notifiche.PublishAsync(g.CapogruppoEmail, "ABSTRACT_ESITO", "Il tuo abstract è stato approvato");
        return "OK";
    }

    public async Task<string> RejectAsync(string docenteEmail, int idGruppo, RifiutaAbstractRequest request)
    {
        var (g, a, err) = await LoadForDocente(docenteEmail, idGruppo);
        if (err is not null) return err;
        if (a!.Esito is not null) return "Abstract già valutato";
        a.Esito = false;
        a.DocenteEmail = docenteEmail;
        a.ModificheProposte = request.ModificheProposte;
        await repo.UpdateAbstractAsync(a);
        if (g!.CapogruppoEmail is not null)
            await notifiche.PublishAsync(g.CapogruppoEmail, "ABSTRACT_ESITO",
                $"Abstract da rivedere: {request.ModificheProposte}");
        return "OK";
    }

    private async Task<(GruppoRef?, Abstract?, string?)> LoadForDocente(string docenteEmail, int idGruppo)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null) return (null, null, "Gruppo non trovato");
        if (docenteEmail != await repo.DocenteDellaSessioneAsync(g.IdSessione)) return (g, null, "Non autorizzato");
        var a = await repo.FindAbstractByGruppoAsync(idGruppo);
        if (a is null) return (g, null, "Abstract non consegnato");
        return (g, a, null);
    }

    private static AbstractDto ToDto(Abstract a) =>
        new(a.IdAbstract, a.IdGruppo, a.Descrizione, a.DataOraConsegna, a.ModificheProposte, a.Esito);
}
