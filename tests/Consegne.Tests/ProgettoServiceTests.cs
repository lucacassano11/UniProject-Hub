using Consegne.Dtos;
using Consegne.Models;
using Consegne.Services;
using Xunit;

public class ProgettoServiceTests
{
    private static ProgettoService Make(out FakeConsegnaRepository repo, StatoGruppo stato = StatoGruppo.ABSTRACT_VALUTATO)
    {
        repo = new FakeConsegnaRepository();
        repo.Gruppi.Add(new GruppoRef { IdGruppo = 1, IdSessione = "SES1", Stato = stato, CapogruppoEmail = "capo@studio.it" });
        repo.DocentiPerSessione["SES1"] = "prof@unibo.it";
        repo.Membri.Add((1, "capo@studio.it"));
        return new ProgettoService(repo, new FakeNotificaPublisher());
    }

    [Fact]
    public async Task Grade_Publishes_Notification_To_Capogruppo()
    {
        var repo = new FakeConsegnaRepository();
        repo.Gruppi.Add(new Consegne.Models.GruppoRef { IdGruppo = 1, IdSessione = "SES1", Stato = Consegne.Models.StatoGruppo.ABSTRACT_VALUTATO, CapogruppoEmail = "capo@studio.it" });
        repo.DocentiPerSessione["SES1"] = "prof@unibo.it";
        repo.Membri.Add((1, "capo@studio.it"));
        var pub = new FakeNotificaPublisher();
        var svc = new ProgettoService(repo, pub);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        await svc.GradeAsync("prof@unibo.it", 1, new(28));
        Assert.Contains(pub.Pubblicate, n => n.Dest == "capo@studio.it" && n.Tipo == "VOTO");
    }

    [Fact]
    public async Task Upload_Requires_Abstract_Approved()
    {
        var svc = Make(out var repo, StatoGruppo.ABSTRACT_CONSEGNATO);
        var dto = await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        Assert.Null(dto);
    }

    [Fact]
    public async Task Upload_By_Capogruppo_Sets_State_PROGETTO_CONSEGNATO()
    {
        var svc = Make(out var repo);
        var dto = await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        Assert.NotNull(dto);
        Assert.Equal(StatoGruppo.PROGETTO_CONSEGNATO, repo.Gruppi.Single().Stato);
    }

    [Fact]
    public async Task Upload_Twice_Refused_Timestamp_Immutable()
    {
        var svc = Make(out var repo);
        var first = await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        var ts = repo.Progetti.Single().DataOraConsegna;
        var second = await svc.UploadAsync("capo@studio.it", 1, new("p2.zip"));
        Assert.Null(second);
        Assert.Single(repo.Progetti);
        Assert.Equal(ts, repo.Progetti.Single().DataOraConsegna);
        Assert.Equal("p.zip", repo.Progetti.Single().FileZip);
    }

    [Fact]
    public async Task Grade_By_Docente_Sets_State_PROGETTO_VALUTATO()
    {
        var svc = Make(out var repo);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        var msg = await svc.GradeAsync("prof@unibo.it", 1, new(28));
        Assert.Equal("OK", msg);
        Assert.Equal(28, repo.Progetti.Single().Valutazione);
        Assert.Equal(StatoGruppo.PROGETTO_VALUTATO, repo.Gruppi.Single().Stato);
    }

    [Fact]
    public async Task Grade_Out_Of_Range_Refused()
    {
        var svc = Make(out var repo);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        Assert.NotEqual("OK", await svc.GradeAsync("prof@unibo.it", 1, new(0)));
        Assert.NotEqual("OK", await svc.GradeAsync("prof@unibo.it", 1, new(34)));
    }

    [Fact]
    public async Task Accept_By_Capogruppo_Sets_Accettazione_True()
    {
        var svc = Make(out var repo);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        await svc.GradeAsync("prof@unibo.it", 1, new(30));
        var msg = await svc.AcceptAsync("capo@studio.it", 1);
        Assert.Equal("OK", msg);
        Assert.True(repo.Progetti.Single().Accettazione);
    }

    [Fact]
    public async Task Reject_By_Capogruppo_Sets_Accettazione_False()
    {
        var svc = Make(out var repo);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        await svc.GradeAsync("prof@unibo.it", 1, new(18));
        var msg = await svc.RejectAsync("capo@studio.it", 1);
        Assert.Equal("OK", msg);
        Assert.False(repo.Progetti.Single().Accettazione);
    }

    [Fact]
    public async Task Accept_Before_Grade_Refused()
    {
        var svc = Make(out var repo);
        await svc.UploadAsync("capo@studio.it", 1, new("p.zip"));
        var msg = await svc.AcceptAsync("capo@studio.it", 1);
        Assert.NotEqual("OK", msg);
    }
}
