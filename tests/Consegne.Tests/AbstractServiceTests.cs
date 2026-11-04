using Consegne.Dtos;
using Consegne.Models;
using Consegne.Services;
using Xunit;

public class AbstractServiceTests
{
    private static AbstractService Make(out FakeConsegnaRepository repo, StatoGruppo stato = StatoGruppo.NUOVO)
    {
        repo = new FakeConsegnaRepository();
        repo.Gruppi.Add(new GruppoRef { IdGruppo = 1, IdSessione = "SES1", Stato = stato, CapogruppoEmail = "capo@studio.it" });
        repo.DocentiPerSessione["SES1"] = "prof@unibo.it";
        repo.Membri.Add((1, "capo@studio.it"));
        return new AbstractService(repo, new FakeNotificaPublisher());
    }

    [Fact]
    public async Task Approve_Publishes_Notification_To_Capogruppo()
    {
        var repo = new FakeConsegnaRepository();
        repo.Gruppi.Add(new Consegne.Models.GruppoRef { IdGruppo = 1, IdSessione = "SES1", Stato = Consegne.Models.StatoGruppo.NUOVO, CapogruppoEmail = "capo@studio.it" });
        repo.DocentiPerSessione["SES1"] = "prof@unibo.it";
        repo.Membri.Add((1, "capo@studio.it"));
        var pub = new FakeNotificaPublisher();
        var svc = new AbstractService(repo, pub);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        await svc.ApproveAsync("prof@unibo.it", 1);
        Assert.Contains(pub.Pubblicate, n => n.Dest == "capo@studio.it" && n.Tipo == "ABSTRACT_ESITO");
    }

    [Fact]
    public async Task Submit_By_Capogruppo_Sets_State_And_Timestamp()
    {
        var svc = Make(out var repo);
        var dto = await svc.SubmitAsync("capo@studio.it", 1, new("la nostra idea"));
        Assert.NotNull(dto);
        Assert.Null(dto!.Esito);
        Assert.NotEqual(default, dto.DataOraConsegna);
        Assert.Equal(StatoGruppo.ABSTRACT_CONSEGNATO, repo.Gruppi.Single().Stato);
    }

    [Fact]
    public async Task Submit_By_Non_Capogruppo_Returns_Null()
    {
        var svc = Make(out var repo);
        var dto = await svc.SubmitAsync("altro@studio.it", 1, new("x"));
        Assert.Null(dto);
    }

    [Fact]
    public async Task Approve_By_Docente_Sets_Esito_True_And_State()
    {
        var svc = Make(out var repo);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        var msg = await svc.ApproveAsync("prof@unibo.it", 1);
        Assert.Equal("OK", msg);
        Assert.True(repo.Abstracts.Single().Esito);
        Assert.Equal(StatoGruppo.ABSTRACT_VALUTATO, repo.Gruppi.Single().Stato);
    }

    [Fact]
    public async Task Approve_By_Other_Docente_Refused()
    {
        var svc = Make(out var repo);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        var msg = await svc.ApproveAsync("intruso@unibo.it", 1);
        Assert.NotEqual("OK", msg);
    }

    [Fact]
    public async Task Reject_Sets_Esito_False_With_Modifiche()
    {
        var svc = Make(out var repo);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        var msg = await svc.RejectAsync("prof@unibo.it", 1, new("rivedere obiettivi"));
        Assert.Equal("OK", msg);
        Assert.False(repo.Abstracts.Single().Esito);
        Assert.Equal("rivedere obiettivi", repo.Abstracts.Single().ModificheProposte);
        Assert.Equal(StatoGruppo.ABSTRACT_CONSEGNATO, repo.Gruppi.Single().Stato);
    }

    [Fact]
    public async Task Resubmit_After_Reject_Resets_Esito_Null()
    {
        var svc = Make(out var repo);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        await svc.RejectAsync("prof@unibo.it", 1, new("rivedere"));
        var dto = await svc.SubmitAsync("capo@studio.it", 1, new("idea migliorata"));
        Assert.NotNull(dto);
        Assert.Null(dto!.Esito);
        Assert.Null(repo.Abstracts.Single().ModificheProposte);
        Assert.Equal("idea migliorata", repo.Abstracts.Single().Descrizione);
    }

    [Fact]
    public async Task Get_Allowed_For_Member_And_Docente()
    {
        var svc = Make(out var repo);
        await svc.SubmitAsync("capo@studio.it", 1, new("idea"));
        Assert.NotNull(await svc.GetAsync("capo@studio.it", 1));
        Assert.NotNull(await svc.GetAsync("prof@unibo.it", 1));
        Assert.Null(await svc.GetAsync("estraneo@studio.it", 1));
    }
}
