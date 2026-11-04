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

public class SicurezzaEndpointsIntegrationTests : IAsyncLifetime
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
            "INSERT INTO utenti(email,password_hash,nome,cognome,ruolo) VALUES" +
            "('anna.s@unibo.it','x','Anna','S','GESTORE_SICUREZZA')," +
            "('stud.s@studio.it','x','Stud','S','STUDENTE');" +
            "INSERT INTO gestori_sicurezza(email) VALUES('anna.s@unibo.it');" +
            "INSERT INTO studenti(email,matricola) VALUES('stud.s@studio.it','6001');";
        await seed.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => _mysql.DisposeAsync().AsTask();

    private WebApplicationFactory<Program> Factory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", _mysql.GetConnectionString());
            b.UseSetting("Jwt:Secret", Secret);
        });

    private HttpClient Client(string email, string role)
    {
        var c = Factory().CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(email, role));
        return c;
    }

    private static string Token(string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("uph", "uph",
            new[] { new Claim(JwtRegisteredClaimNames.Sub, email), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private record UtenteDtoTest(string email, string ruolo, int failedAttempts, string? lockedUntil, bool bloccato);

    [Fact]
    public async Task Gestore_Blocca_E_Sblocca_Con_Log()
    {
        var g = Client("anna.s@unibo.it", "GESTORE_SICUREZZA");

        var utenti = await g.GetFromJsonAsync<List<UtenteDtoTest>>("/utenti");
        Assert.Contains(utenti!, u => u.email == "stud.s@studio.it");

        var blocca = await g.PostAsync("/utenti/stud.s@studio.it/blocca", null);
        Assert.Equal(HttpStatusCode.NoContent, blocca.StatusCode);

        var dopoBlocco = await g.GetFromJsonAsync<List<UtenteDtoTest>>("/utenti");
        Assert.True(dopoBlocco!.Single(u => u.email == "stud.s@studio.it").bloccato);

        var sblocca = await g.PostAsync("/utenti/stud.s@studio.it/sblocca", null);
        Assert.Equal(HttpStatusCode.NoContent, sblocca.StatusCode);

        var dopoSblocco = await g.GetFromJsonAsync<List<UtenteDtoTest>>("/utenti");
        Assert.False(dopoSblocco!.Single(u => u.email == "stud.s@studio.it").bloccato);

        // verifica che gli eventi siano stati scritti in log_eventi
        await using var conn = new MySqlConnection(_mysql.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM log_eventi WHERE tipo IN ('BLOCCO_MANUALE','SBLOCCO_MANUALE')";
        var n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        Assert.Equal(2, n);
    }

    [Fact]
    public async Task Studente_Is_Forbidden()
    {
        var res = await Client("stud.s@studio.it", "STUDENTE").GetAsync("/utenti");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var res = await Factory().CreateClient().GetAsync("/utenti");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
