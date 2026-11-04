using System.IdentityModel.Tokens.Jwt;
using Auth.Models;
using Auth.Services;
using Xunit;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_Embeds_Email_And_Role()
    {
        var svc = new JwtService("test-secret-at-least-32-chars-long-xx", "uph", "uph");

        var token = svc.GenerateToken("mario.rossi@unibo.it", Ruolo.DOCENTE);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("mario.rossi@unibo.it", jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("DOCENTE", jwt.Claims.First(c => c.Type == "role").Value);
    }
}
