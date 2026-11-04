using Sessioni.Models;
using Sessioni.Services;
using Xunit;

public class SessionCleanupTests
{
    private static readonly DateOnly Now = new(2026, 6, 1);

    [Fact]
    public async Task Deletes_Old_Session_Without_Groups()
    {
        var repo = new FakeSessionRepository();
        repo.Sessioni.Add(new Sessione { IdSessione = "OLD12345", DocenteEmail = "p@unibo.it",
            DataCreazioneSessione = Now.AddMonths(-7) });
        var cleanup = new SessionCleanup(repo);

        var deleted = await cleanup.RunOnceAsync(Now);

        Assert.Equal(1, deleted);
        Assert.Empty(repo.Sessioni);
    }

    [Fact]
    public async Task Keeps_Old_Session_With_Groups()
    {
        var repo = new FakeSessionRepository();
        repo.Sessioni.Add(new Sessione { IdSessione = "OLD12345", DocenteEmail = "p@unibo.it",
            DataCreazioneSessione = Now.AddMonths(-7) });
        repo.GruppiCount["OLD12345"] = 1;
        var cleanup = new SessionCleanup(repo);

        var deleted = await cleanup.RunOnceAsync(Now);

        Assert.Equal(0, deleted);
        Assert.Single(repo.Sessioni);
    }

    [Fact]
    public async Task Keeps_Recent_Session()
    {
        var repo = new FakeSessionRepository();
        repo.Sessioni.Add(new Sessione { IdSessione = "NEW12345", DocenteEmail = "p@unibo.it",
            DataCreazioneSessione = Now.AddMonths(-2) });
        var cleanup = new SessionCleanup(repo);

        var deleted = await cleanup.RunOnceAsync(Now);

        Assert.Equal(0, deleted);
        Assert.Single(repo.Sessioni);
    }
}
