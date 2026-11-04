using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Testcontainers.MySql;
using Xunit;

public class LogEndpointsIntegrationTests : IAsyncLifetime
{
    private const string Secret = "test-secret-at-least-32-chars-long-xx";
    private readonly MySqlContainer _mysql = new MySqlBuilder().WithImage("mysql:8")
        .WithDatabase("uniprojecthub").Build();

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();
        var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "init.sql"));
        await using var conn = new MySqlConnection(_mysql.GetConnectionString());
        await conn.OpenAsync();
        foreach (var raw in sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var stmt = string.Join('\n', raw.Split('\n').Where(l => !l.TrimStart().StartsWith("--"))).Trim();
            if (stmt.Length == 0) continue;
            if (stmt.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase)) continue;
            if (stmt.StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) continue;
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt;
            await cmd.ExecuteNonQueryAsync();
        }
        await using var seed = conn.CreateCommand();
        seed.CommandText =
            "INSERT INTO log_eventi(tipo,attore_email,dettaglio) VALUES" +
            "('LOGIN_FALLITO','a@studio.it','tentativo 1')," +
            "('VOTO','prof@unibo.it','voto 28')," +
            "('BLOCCO_MANUALE','anna.bianchi@unibo.it','bloccato a@studio.it');";
        await seed.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => _mysql.DisposeAsync().AsTask();

    private WebApplicationFactory<Program> Factory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", _mysql.GetConnectionString());
            b.UseSetting("Jwt:Secret", Secret);
        });

    private HttpClient Client(string role)
    {
        var c = Factory().CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(role));
        return c;
    }

    private static string Token(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("uph", "uph",
            new[] { new Claim(JwtRegisteredClaimNames.Sub, "anna.bianchi@unibo.it"), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private record EventoDtoTest(int idEvento, string tipo, string? attoreEmail, string dettaglio, string createdAt);

    [Fact]
    public async Task Gestore_Reads_Events()
    {
        var c = Client("GESTORE_SICUREZZA");
        var all = await c.GetFromJsonAsync<List<EventoDtoTest>>("/eventi");
        Assert.Equal(3, all!.Count);

        var soloLogin = await c.GetFromJsonAsync<List<EventoDtoTest>>("/eventi?tipo=LOGIN_FALLITO");
        Assert.Single(soloLogin!);
    }

    [Fact]
    public async Task Non_Gestore_Is_Forbidden()
    {
        var res = await Client("STUDENTE").GetAsync("/eventi");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var res = await Factory().CreateClient().GetAsync("/eventi");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
