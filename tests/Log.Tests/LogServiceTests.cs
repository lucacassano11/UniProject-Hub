using Log.Services;
using Xunit;

public class LogServiceTests
{
    [Fact]
    public async Task List_Returns_Newest_First()
    {
        var repo = new FakeLogRepository();
        repo.Seed("LOGIN_FALLITO", "a@x.it", "vecchio", new DateTime(2026, 1, 1));
        repo.Seed("VOTO", "b@x.it", "nuovo", new DateTime(2026, 6, 1));
        var svc = new LogService(repo);
        var list = await svc.ListAsync(null, 100);
        Assert.Equal("nuovo", list[0].Dettaglio);
    }

    [Fact]
    public async Task List_Filters_By_Tipo()
    {
        var repo = new FakeLogRepository();
        repo.Seed("LOGIN_FALLITO", "a@x.it", "x");
        repo.Seed("VOTO", "b@x.it", "y");
        var svc = new LogService(repo);
        var list = await svc.ListAsync("VOTO", 100);
        Assert.Single(list);
        Assert.Equal("VOTO", list[0].Tipo);
    }

    [Fact]
    public async Task List_Respects_Limit()
    {
        var repo = new FakeLogRepository();
        for (var i = 0; i < 10; i++) repo.Seed("X", null, $"e{i}");
        var svc = new LogService(repo);
        var list = await svc.ListAsync(null, 3);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task List_Invalid_Limit_Defaults_To_100()
    {
        var repo = new FakeLogRepository();
        for (var i = 0; i < 120; i++) repo.Seed("X", null, $"e{i}");
        var svc = new LogService(repo);
        var list = await svc.ListAsync(null, 0);
        Assert.Equal(100, list.Count);
    }
}
