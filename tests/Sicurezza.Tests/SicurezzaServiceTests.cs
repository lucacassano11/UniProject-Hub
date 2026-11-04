using Sicurezza.Models;
using Sicurezza.Services;
using Xunit;

public class SicurezzaServiceTests
{
    private static SicurezzaService Make(out FakeSicurezzaRepository repo, out FakeEventoLogger log)
    {
        repo = new FakeSicurezzaRepository();
        repo.Utenti.Add(new UtenteRef { Email = "stud@studio.it", Ruolo = "STUDENTE", FailedAttempts = 2 });
        log = new FakeEventoLogger();
        return new SicurezzaService(repo, log);
    }

    [Fact]
    public async Task List_Maps_Bloccato_State()
    {
        var svc = Make(out var repo, out _);
        repo.Utenti.Add(new UtenteRef { Email = "lock@studio.it", Ruolo = "STUDENTE", LockedUntil = DateTime.UtcNow.AddHours(1) });
        var list = await svc.ListUtentiAsync();
        Assert.True(list.Single(u => u.Email == "lock@studio.it").Bloccato);
        Assert.False(list.Single(u => u.Email == "stud@studio.it").Bloccato);
    }

    [Fact]
    public async Task Blocca_Sets_LockedUntil_And_Logs()
    {
        var svc = Make(out var repo, out var log);
        var ok = await svc.BloccaAsync("anna@unibo.it", "stud@studio.it");
        Assert.True(ok);
        Assert.NotNull(repo.Utenti.Single().LockedUntil);
        Assert.True(repo.Utenti.Single().LockedUntil > DateTime.UtcNow);
        Assert.Contains(log.Eventi, e => e.Tipo == "BLOCCO_MANUALE" && e.Attore == "anna@unibo.it");
    }

    [Fact]
    public async Task Sblocca_Resets_And_Logs()
    {
        var svc = Make(out var repo, out var log);
        repo.Utenti.Single().LockedUntil = DateTime.UtcNow.AddYears(1);
        var ok = await svc.SbloccaAsync("anna@unibo.it", "stud@studio.it");
        Assert.True(ok);
        Assert.Null(repo.Utenti.Single().LockedUntil);
        Assert.Equal(0, repo.Utenti.Single().FailedAttempts);
        Assert.Contains(log.Eventi, e => e.Tipo == "SBLOCCO_MANUALE");
    }

    [Fact]
    public async Task Blocca_Unknown_User_Returns_False()
    {
        var svc = Make(out var repo, out _);
        Assert.False(await svc.BloccaAsync("anna@unibo.it", "ghost@studio.it"));
    }
}
