using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Services;

public class JwtService(string secret, string issuer, string audience) : IJwtService
{
    public string GenerateToken(string email, Ruolo ruolo)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim("role", ruolo.ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: issuer, audience: audience, claims: claims,
            expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
