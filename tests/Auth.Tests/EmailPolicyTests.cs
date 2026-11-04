using Auth.Models;
using Auth.Services;
using Xunit;

public class EmailPolicyTests
{
    private readonly EmailPolicy _policy = new();

    [Theory]
    [InlineData("mario.rossi@studio.unibo.it", true)]
    [InlineData("mario.rossi@unibo.it", true)]
    [InlineData("mario.rossi@gmail.com", false)]
    [InlineData("mario.rossi@studenti.example.it", false)]
    public void IsUniversity_Checks_Domain(string email, bool expected)
    {
        Assert.Equal(expected, _policy.IsUniversityEmail(email));
    }

    [Theory]
    [InlineData("a.b@studio.unibo.it", Ruolo.STUDENTE)]
    [InlineData("a.b@unibo.it", Ruolo.DOCENTE)]
    public void RoleFromEmail_Maps_Domain(string email, Ruolo expected)
    {
        Assert.Equal(expected, _policy.RoleFromEmail(email));
    }

    [Fact]
    public void DeriveName_Splits_LocalPart()
    {
        var (nome, cognome) = _policy.DeriveName("luca.cassano@studio.unibo.it");
        Assert.Equal("Luca", nome);
        Assert.Equal("Cassano", cognome);
    }
}
