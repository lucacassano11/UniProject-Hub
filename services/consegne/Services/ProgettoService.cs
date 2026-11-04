using Consegne.Data;
using Consegne.Dtos;
using Consegne.Models;
using UniProjectHub.Shared;

namespace Consegne.Services;

public class ProgettoService(IConsegnaRepository repo, INotificaPublisher notifiche) : IProgettoService
{
    public async Task<ProgettoDto?> UploadAsync(string studenteEmail, int idGruppo, UploadProgettoRequest request)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != studenteEmail) return null;
        if (g.Stato != StatoGruppo.ABSTRACT_VALUTATO) return null;
        if (await repo.FindProgettoByGruppoAsync(idGruppo) is not null) return null; // timestamp immutabile: 1 sola consegna

        var p = new Progetto
        {
            IdGruppo = idGruppo,
            FileZip = request.FileZip,
            DataOraConsegna = DateTime.UtcNow,
        };
        await repo.AddProgettoAsync(p);
        g.Stato = StatoGruppo.PROGETTO_CONSEGNATO;
        await repo.UpdateGruppoStatoAsync(g);
        return ToDto(p);
    }

    public async Task<ProgettoDto?> GetAsync(string email, int idGruppo)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null) return null;
        var autorizzato = await repo.IsMembroAsync(idGruppo, email)
            || email == await repo.DocenteDellaSessioneAsync(g.IdSessione);
        if (!autorizzato) return null;
        var p = await repo.FindProgettoByGruppoAsync(idGruppo);
        return p is null ? null : ToDto(p);
    }

    public async Task<string> GradeAsync(string docenteEmail, int idGruppo, ValutaRequest request)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null) return "Gruppo non trovato";
        if (docenteEmail != await repo.DocenteDellaSessioneAsync(g.IdSessione)) return "Non autorizzato";
        if (request.Valutazione is < 1 or > 33) return "Voto fuori range";
        var p = await repo.FindProgettoByGruppoAsync(idGruppo);
        if (p is null) return "Progetto non consegnato";
        if (g.Stato != StatoGruppo.PROGETTO_CONSEGNATO) return "Progetto non in stato valutabile";
        p.Valutazione = request.Valutazione;
        p.DocenteEmail = docenteEmail;
        await repo.UpdateProgettoAsync(p);
        g.Stato = StatoGruppo.PROGETTO_VALUTATO;
        await repo.UpdateGruppoStatoAsync(g);
        if (g.CapogruppoEmail is not null)
            await notifiche.PublishAsync(g.CapogruppoEmail, "VOTO", $"Hai ricevuto la valutazione {request.Valutazione}");
        return "OK";
    }

    public Task<string> AcceptAsync(string studenteEmail, int idGruppo) => SetAccettazione(studenteEmail, idGruppo, true);
    public Task<string> RejectAsync(string studenteEmail, int idGruppo) => SetAccettazione(studenteEmail, idGruppo, false);

    private async Task<string> SetAccettazione(string studenteEmail, int idGruppo, bool valore)
    {
        var g = await repo.FindGruppoAsync(idGruppo);
        if (g is null || g.CapogruppoEmail != studenteEmail) return "Non autorizzato";
        var p = await repo.FindProgettoByGruppoAsync(idGruppo);
        if (p is null || p.Valutazione is null) return "Progetto non ancora valutato";
        p.Accettazione = valore;
        await repo.UpdateProgettoAsync(p);
        return "OK";
    }

    private static ProgettoDto ToDto(Progetto p) =>
        new(p.IdProgetto, p.IdGruppo, p.FileZip, p.DataOraConsegna, p.Valutazione, p.Accettazione);
}
