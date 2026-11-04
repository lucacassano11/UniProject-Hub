using Notifiche.Services;
using Xunit;

public class NotificaServiceTests
{
    private static NotificaService Make(out FakeNotificheRepository repo)
    {
        repo = new FakeNotificheRepository();
        return new NotificaService(repo);
    }

    [Fact]
    public async Task List_Returns_Only_Own_Notifications()
    {
        var svc = Make(out var repo);
        repo.Seed("a@x.it", "INVITO", "m1");
        repo.Seed("b@x.it", "INVITO", "m2");
        var list = await svc.ListAsync("a@x.it", soloNonLette: false);
        Assert.Single(list);
    }

    [Fact]
    public async Task List_SoloNonLette_Filters_Read()
    {
        var svc = Make(out var repo);
        repo.Seed("a@x.it", "VOTO", "letta", letta: true);
        repo.Seed("a@x.it", "VOTO", "nuova");
        var list = await svc.ListAsync("a@x.it", soloNonLette: true);
        Assert.Single(list);
        Assert.Equal("nuova", list[0].Messaggio);
    }

    [Fact]
    public async Task Count_NonLette_Is_Correct()
    {
        var svc = Make(out var repo);
        repo.Seed("a@x.it", "VOTO", "1");
        repo.Seed("a@x.it", "VOTO", "2");
        repo.Seed("a@x.it", "VOTO", "3", letta: true);
        Assert.Equal(2, await svc.CountNonLetteAsync("a@x.it"));
    }

    [Fact]
    public async Task MarkLetta_Own_Succeeds()
    {
        var svc = Make(out var repo);
        var n = repo.Seed("a@x.it", "VOTO", "m");
        var ok = await svc.MarkLettaAsync("a@x.it", n.IdNotifica);
        Assert.True(ok);
        Assert.True(repo.Notifiche.Single().Letta);
    }

    [Fact]
    public async Task MarkLetta_Other_Users_Is_Refused()
    {
        var svc = Make(out var repo);
        var n = repo.Seed("a@x.it", "VOTO", "m");
        var ok = await svc.MarkLettaAsync("intruso@x.it", n.IdNotifica);
        Assert.False(ok);
        Assert.False(repo.Notifiche.Single().Letta);
    }
}
