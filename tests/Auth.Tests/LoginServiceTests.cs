using Auth.Dtos;
using Auth.Models;
using Auth.Services;
using Xunit;

public class LoginServiceTests
{
    private static (LoginService svc, FakeUserRepository repo) Make()
    {
        var repo = new FakeUserRepository();
        var pwd = new PasswordPolicy();
        repo.Utenti.Add(new Utente
        {
            Email = "a.b@studio.unibo.it",
            PasswordHash = pwd.Hash("Abcdef1!"),
            Nome = "A", Cognome = "B", Ruolo = Ruolo.STUDENTE,
        });
        var jwt = new JwtService("test-secret-at-least-32-chars-long-xx", "uph", "uph");
        return (new LoginService(repo, pwd, jwt, new FakeEventoLogger()), repo);
    }

    [Fact]
    public async Task Login_Valid_Returns_Token()
    {
        var (svc, _) = Make();
        var res = await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Abcdef1!"));
        Assert.True(res.Success);
        Assert.False(string.IsNullOrEmpty(res.User!.Token));
    }

    [Fact]
    public async Task Login_Wrong_Password_Fails_And_Increments()
    {
        var (svc, repo) = Make();
        var res = await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Wrong1!A"));
        Assert.False(res.Success);
        Assert.Equal(1, repo.Utenti[0].FailedAttempts);
    }

    [Fact]
    public async Task Login_Locks_After_Three_Failures()
    {
        var (svc, repo) = Make();
        for (var i = 0; i < 3; i++)
            await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Wrong1!A"));

        Assert.NotNull(repo.Utenti[0].LockedUntil);

        var res = await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Abcdef1!"));
        Assert.False(res.Success);
        Assert.Contains("blocc", res.Error, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_Success_Resets_FailedAttempts()
    {
        var (svc, repo) = Make();
        await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Wrong1!A"));
        await svc.LoginAsync(new LoginRequest("a.b@studio.unibo.it", "Abcdef1!"));
        Assert.Equal(0, repo.Utenti[0].FailedAttempts);
    }
}
