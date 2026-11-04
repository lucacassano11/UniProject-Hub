using Gruppi.Services;
using Xunit;

public class InvitoServiceTests
{
    private static (GruppoService g, InvitoService i, FakeGruppoRepository repo) Make()
    {
        var repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.Studenti.UnionWith(new[] { "capo@studio.it", "inv@studio.it", "x@studio.it", "y@studio.it" });
        return (new GruppoService(repo), new InvitoService(repo, new FakeNotificaPublisher()), repo);
    }

    [Fact]
    public async Task Invite_Publishes_Notification_To_Invited()
    {
        var repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.Studenti.UnionWith(new[] { "capo@studio.it", "inv@studio.it" });
        var pub = new FakeNotificaPublisher();
        var g = new GruppoService(repo);
        var i = new InvitoService(repo, pub);
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        Assert.Contains(pub.Pubblicate, n => n.Dest == "inv@studio.it" && n.Tipo == "INVITO");
    }

    [Fact]
    public async Task Capogruppo_Can_Invite_Existing_Student()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        Assert.NotNull(dto);
        Assert.Null(dto!.Esito);
    }

    [Fact]
    public async Task Non_Capogruppo_Cannot_Invite()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await i.InviteAsync("x@studio.it", grp.IdGruppo, new("inv@studio.it"));
        Assert.Null(dto);
    }

    [Fact]
    public async Task Accept_Invite_Adds_Member()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        var msg = await i.AcceptAsync("inv@studio.it", dto!.IdRichiesta);
        Assert.Equal("OK", msg);
        Assert.True(await repo.IsMembroAsync(grp.IdGruppo, "inv@studio.it"));
        Assert.True(repo.Richieste.Single().Esito);
    }

    [Fact]
    public async Task Reject_Invite_Sets_Esito_False()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        var msg = await i.RejectAsync("inv@studio.it", dto!.IdRichiesta);
        Assert.Equal("OK", msg);
        Assert.False(repo.Richieste.Single().Esito);
        Assert.False(await repo.IsMembroAsync(grp.IdGruppo, "inv@studio.it"));
    }

    [Fact]
    public async Task Accept_Fails_When_Group_Full()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        // riempi fino a dimensione 3 (capo + 2)
        repo.Membri.Add(new() { IdGruppo = grp.IdGruppo, StudenteEmail = "x@studio.it", DataIngresso = DateTime.UtcNow });
        repo.Membri.Add(new() { IdGruppo = grp.IdGruppo, StudenteEmail = "y@studio.it", DataIngresso = DateTime.UtcNow });
        var dto = await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        var msg = await i.AcceptAsync("inv@studio.it", dto!.IdRichiesta);
        Assert.NotEqual("OK", msg);
    }

    [Fact]
    public async Task Accept_Fails_If_Already_In_Another_Group_In_Session()
    {
        var (g, i, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        await g.CreateAsync("inv@studio.it", new("SES1")); // inv è già in un gruppo della stessa sessione
        var dto = await i.InviteAsync("capo@studio.it", grp.IdGruppo, new("inv@studio.it"));
        var msg = await i.AcceptAsync("inv@studio.it", dto!.IdRichiesta);
        Assert.NotEqual("OK", msg);
    }
}
