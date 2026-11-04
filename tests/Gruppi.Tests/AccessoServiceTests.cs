using Gruppi.Services;
using Xunit;

public class AccessoServiceTests
{
    private static (GruppoService g, AccessoService a, FakeGruppoRepository repo) Make()
    {
        var repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.Studenti.UnionWith(new[] { "capo@studio.it", "req@studio.it" });
        return (new GruppoService(repo), new AccessoService(repo, new FakeNotificaPublisher()), repo);
    }

    [Fact]
    public async Task Request_Publishes_Notification_To_Capogruppo()
    {
        var repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.Studenti.UnionWith(new[] { "capo@studio.it", "req@studio.it" });
        var pub = new FakeNotificaPublisher();
        var g = new GruppoService(repo);
        var a = new AccessoService(repo, pub);
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        await a.RequestAsync("req@studio.it", grp.IdGruppo);
        Assert.Contains(pub.Pubblicate, n => n.Dest == "capo@studio.it" && n.Tipo == "RICHIESTA_ACCESSO");
    }

    [Fact]
    public async Task Student_Can_Request_Access()
    {
        var (g, a, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await a.RequestAsync("req@studio.it", grp.IdGruppo);
        Assert.NotNull(dto);
        Assert.Null(dto!.Esito);
    }

    [Fact]
    public async Task Request_Fails_If_Already_In_Group_In_Session()
    {
        var (g, a, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        await g.CreateAsync("req@studio.it", new("SES1"));
        var dto = await a.RequestAsync("req@studio.it", grp.IdGruppo);
        Assert.Null(dto);
    }

    [Fact]
    public async Task Capogruppo_Accepts_Access_Adds_Member()
    {
        var (g, a, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await a.RequestAsync("req@studio.it", grp.IdGruppo);
        var msg = await a.AcceptAsync("capo@studio.it", dto!.IdRichiesta);
        Assert.Equal("OK", msg);
        Assert.True(await repo.IsMembroAsync(grp.IdGruppo, "req@studio.it"));
    }

    [Fact]
    public async Task Non_Capogruppo_Cannot_Decide()
    {
        var (g, a, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await a.RequestAsync("req@studio.it", grp.IdGruppo);
        var msg = await a.AcceptAsync("req@studio.it", dto!.IdRichiesta);
        Assert.NotEqual("OK", msg);
    }

    [Fact]
    public async Task Reject_Access_Sets_Esito_False()
    {
        var (g, a, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await a.RequestAsync("req@studio.it", grp.IdGruppo);
        var msg = await a.RejectAsync("capo@studio.it", dto!.IdRichiesta);
        Assert.Equal("OK", msg);
        Assert.False(repo.Richieste.Single().Esito);
    }
}
