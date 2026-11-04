using Sessioni.Dtos;
using Sessioni.Services;
using Xunit;

public class SessionServiceTests
{
    private static SessionService Make(out FakeSessionRepository repo)
    {
        repo = new FakeSessionRepository();
        return new SessionService(repo, new SessionCodeGenerator());
    }

    [Fact]
    public async Task Create_Adds_Session_For_Docente()
    {
        var svc = Make(out var repo);
        var dto = await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        Assert.Single(repo.Sessioni);
        Assert.Equal("prof@unibo.it", repo.Sessioni[0].DocenteEmail);
        Assert.Equal(8, dto.IdSessione.Length);
        Assert.Equal(30, dto.VotoMassimo);
    }

    [Fact]
    public async Task List_Returns_Only_Own_Sessions()
    {
        var svc = Make(out var repo);
        await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        await svc.CreateAsync("altro@unibo.it", new CreateSessionRequest(28));
        var mine = await svc.ListAsync("prof@unibo.it");
        Assert.Single(mine);
    }

    [Fact]
    public async Task SetInizioAbstract_Updates_Field()
    {
        var svc = Make(out var repo);
        var s = await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        var ok = await svc.SetInizioAbstractAsync("prof@unibo.it", s.IdSessione,
            new InizioAbstractRequest(new DateOnly(2026, 4, 1)));
        Assert.True(ok);
        Assert.Equal(new DateOnly(2026, 4, 1), repo.Sessioni[0].InizioInvioAbstract);
    }

    [Fact]
    public async Task Delete_Own_Session_Without_Groups_Succeeds()
    {
        var svc = Make(out var repo);
        var s = await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        var msg = await svc.DeleteAsync("prof@unibo.it", s.IdSessione);
        Assert.Equal("OK", msg);
        Assert.Empty(repo.Sessioni);
    }

    [Fact]
    public async Task Delete_Session_With_Groups_Is_Refused()
    {
        var svc = Make(out var repo);
        var s = await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        repo.GruppiCount[s.IdSessione] = 2;
        var msg = await svc.DeleteAsync("prof@unibo.it", s.IdSessione);
        Assert.NotEqual("OK", msg);
        Assert.Single(repo.Sessioni);
    }

    [Fact]
    public async Task Delete_Other_Docente_Session_Is_Refused()
    {
        var svc = Make(out var repo);
        var s = await svc.CreateAsync("prof@unibo.it", new CreateSessionRequest(30));
        var msg = await svc.DeleteAsync("intruso@unibo.it", s.IdSessione);
        Assert.NotEqual("OK", msg);
        Assert.Single(repo.Sessioni);
    }
}
