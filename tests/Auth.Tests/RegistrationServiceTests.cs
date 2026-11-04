using Auth.Dtos;
using Auth.Services;
using Xunit;

public class RegistrationServiceTests
{
    private static RegistrationService Make(out FakeUserRepository repo)
    {
        repo = new FakeUserRepository();
        return new RegistrationService(repo, new PasswordPolicy(), new EmailPolicy());
    }

    [Fact]
    public async Task Register_Student_Creates_User_With_Matricola()
    {
        var svc = Make(out var repo);
        var res = await svc.RegisterAsync(new RegisterRequest("luca.cassano@studio.unibo.it", "Abcdef1!", "STUDENTE"));
        Assert.True(res.Success);
        Assert.Single(repo.Utenti);
        Assert.Equal("Luca", repo.Utenti[0].Nome);
        Assert.True(repo.Matricole.ContainsKey("luca.cassano@studio.unibo.it"));
    }

    [Fact]
    public async Task Register_Rejects_NonUniversity_Email()
    {
        var svc = Make(out _);
        var res = await svc.RegisterAsync(new RegisterRequest("x.y@gmail.com", "Abcdef1!", "STUDENTE"));
        Assert.False(res.Success);
        Assert.Contains("universitaria", res.Error, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_Rejects_Role_Domain_Mismatch()
    {
        var svc = Make(out _);
        var res = await svc.RegisterAsync(new RegisterRequest("a.b@unibo.it", "Abcdef1!", "STUDENTE"));
        Assert.False(res.Success);
    }

    [Fact]
    public async Task Register_Rejects_Weak_Password()
    {
        var svc = Make(out _);
        var res = await svc.RegisterAsync(new RegisterRequest("a.b@studio.unibo.it", "weak", "STUDENTE"));
        Assert.False(res.Success);
    }

    [Fact]
    public async Task Register_Rejects_Duplicate_Email()
    {
        var svc = Make(out var repo);
        await svc.RegisterAsync(new RegisterRequest("a.b@studio.unibo.it", "Abcdef1!", "STUDENTE"));
        var res = await svc.RegisterAsync(new RegisterRequest("a.b@studio.unibo.it", "Abcdef1!", "STUDENTE"));
        Assert.False(res.Success);
        Assert.Single(repo.Utenti);
    }
}
