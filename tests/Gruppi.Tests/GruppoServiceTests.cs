using Gruppi.Dtos;
using Gruppi.Services;
using Xunit;

public class GruppoServiceTests
{
    private static GruppoService Make(out FakeGruppoRepository repo)
    {
        repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.Studenti.Add("a@studio.it");
        repo.Studenti.Add("b@studio.it");
        return new GruppoService(repo);
    }

    [Fact]
    public async Task Create_Makes_Caller_Capogruppo_And_First_Member()
    {
        var svc = Make(out var repo);
        var dto = await svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1"));
        Assert.Equal("a@studio.it", dto.CapogruppoEmail);
        Assert.Equal(1, dto.NumeroMembri);
        Assert.True(dto.Membri.Single().IsCapogruppo);
        Assert.Equal("NUOVO", dto.Stato);
    }

    [Fact]
    public async Task Create_Fails_If_Session_Missing()
    {
        var svc = Make(out var repo);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync("a@studio.it", new CreateGruppoRequest("NOPE")));
    }

    [Fact]
    public async Task Create_Fails_If_Already_In_Group_In_Session()
    {
        var svc = Make(out var repo);
        await svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1")));
    }

    [Fact]
    public async Task Leave_Removes_Member()
    {
        var svc = Make(out var repo);
        var g = await svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1"));
        repo.Membri.Add(new() { IdGruppo = g.IdGruppo, StudenteEmail = "b@studio.it", DataIngresso = DateTime.UtcNow });
        var msg = await svc.LeaveAsync("b@studio.it", g.IdGruppo);
        Assert.Equal("OK", msg);
        Assert.Single(repo.Membri, m => m.IdGruppo == g.IdGruppo);
    }

    [Fact]
    public async Task Leave_By_Capogruppo_Transfers_Leadership()
    {
        var svc = Make(out var repo);
        var g = await svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1"));
        repo.Membri.Add(new() { IdGruppo = g.IdGruppo, StudenteEmail = "b@studio.it", DataIngresso = DateTime.UtcNow.AddMinutes(1) });
        var msg = await svc.LeaveAsync("a@studio.it", g.IdGruppo);
        Assert.Equal("OK", msg);
        Assert.Equal("b@studio.it", repo.Gruppi.Single().CapogruppoEmail);
    }

    [Fact]
    public async Task Leave_Last_Member_Deletes_Group()
    {
        var svc = Make(out var repo);
        var g = await svc.CreateAsync("a@studio.it", new CreateGruppoRequest("SES1"));
        var msg = await svc.LeaveAsync("a@studio.it", g.IdGruppo);
        Assert.Equal("OK", msg);
        Assert.Empty(repo.Gruppi);
    }
}
