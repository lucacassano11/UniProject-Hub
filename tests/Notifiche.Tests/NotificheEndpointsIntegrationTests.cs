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

public class NotificheEndpointsIntegrationTests : IAsyncLifetime
{
    private const string Secret = "test-secret-at-least-32-chars-long-xx";
    private readonly MySqlContainer _mysql = new MySqlBuilder().WithImage("mysql:8")
        .WithDatabase("uniprojecthub").Build();
    private int _idNonLetta;

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
        await using (var seed = conn.CreateCommand())
        {
            seed.CommandText =
                "INSERT INTO utenti(email,password_hash,nome,cognome,ruolo) VALUES" +
                "('n1@studio.it','x','N','Uno','STUDENTE'),('n2@studio.it','x','N','Due','STUDENTE');" +
                "INSERT INTO studenti(email,matricola) VALUES('n1@studio.it','8001'),('n2@studio.it','8002');" +
                "INSERT INTO notifiche(destinatario_email,tipo,messaggio,letta) VALUES" +
                "('n1@studio.it','INVITO','invito al gruppo 1',FALSE)," +
                "('n1@studio.it','VOTO','voto assegnato',TRUE)," +
                "('n2@studio.it','INVITO','non tua',FALSE);";
            await seed.ExecuteNonQueryAsync();
        }
        await using (var q = conn.CreateCommand())
        {
            q.CommandText = "SELECT id_notifica FROM notifiche WHERE destinatario_email='n1@studio.it' AND letta=FALSE";
            _idNonLetta = Convert.ToInt32(await q.ExecuteScalarAsync());
        }
    }

    public Task DisposeAsync() => _mysql.DisposeAsync().AsTask();

    private WebApplicationFactory<Program> Factory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", _mysql.GetConnectionString());
            b.UseSetting("Jwt:Secret", Secret);
        });

    private HttpClient Client(string email)
    {
        var c = Factory().CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(email));
        return c;
    }

    private static string Token(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("uph", "uph",
            new[] { new Claim(JwtRegisteredClaimNames.Sub, email), new Claim("role", "STUDENTE") },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private record NotificaDtoTest(int idNotifica, string tipo, string messaggio, bool letta, string createdAt);
    private record ConteggioTest(int nonLette);

    [Fact]
    public async Task List_And_Count_And_MarkRead_Flow()
    {
        var c = Client("n1@studio.it");

        var soloNonLette = await c.GetFromJsonAsync<List<NotificaDtoTest>>("/notifiche?soloNonLette=true");
        Assert.Single(soloNonLette!);

        var tutte = await c.GetFromJsonAsync<List<NotificaDtoTest>>("/notifiche");
        Assert.Equal(2, tutte!.Count);

        var conteggio = await c.GetFromJsonAsync<ConteggioTest>("/notifiche/conteggio");
        Assert.Equal(1, conteggio!.nonLette);

        var letta = await c.PostAsync($"/notifiche/{_idNonLetta}/letta", null);
        Assert.Equal(HttpStatusCode.NoContent, letta.StatusCode);

        var dopo = await c.GetFromJsonAsync<ConteggioTest>("/notifiche/conteggio");
        Assert.Equal(0, dopo!.nonLette);
    }

    [Fact]
    public async Task MarkRead_Other_Users_Notification_Is_NotFound()
    {
        var c = Client("n2@studio.it");
        var res = await c.PostAsync($"/notifiche/{_idNonLetta}/letta", null); // appartiene a n1
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var res = await Factory().CreateClient().GetAsync("/notifiche");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
