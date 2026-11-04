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

public class ConsegneEndpointsIntegrationTests : IAsyncLifetime
{
    private const string Secret = "test-secret-at-least-32-chars-long-xx";
    private readonly MySqlContainer _mysql = new MySqlBuilder().WithImage("mysql:8")
        .WithDatabase("uniprojecthub").Build();
    private int _idGruppo;

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
        // seed: docente + sessione + studente + gruppo (capogruppo c1)
        await using (var seed = conn.CreateCommand())
        {
            seed.CommandText =
                "INSERT INTO utenti(email,password_hash,nome,cognome,ruolo) VALUES" +
                "('prof.c@unibo.it','x','P','C','DOCENTE'),('c1@studio.it','x','C','Uno','STUDENTE');" +
                "INSERT INTO docenti(email) VALUES('prof.c@unibo.it');" +
                "INSERT INTO studenti(email,matricola) VALUES('c1@studio.it','7001');" +
                "INSERT INTO sessioni(id_sessione,voto_massimo,data_creazione_sessione,docente_email) " +
                "VALUES('CSES0001',30,'2026-03-01','prof.c@unibo.it');" +
                "INSERT INTO gruppi(id_sessione,dimensione,stato,capogruppo_email) " +
                "VALUES('CSES0001',3,'NUOVO','c1@studio.it');";
            await seed.ExecuteNonQueryAsync();
        }
        await using (var q = conn.CreateCommand())
        {
            q.CommandText = "SELECT id_gruppo FROM gruppi WHERE capogruppo_email='c1@studio.it'";
            _idGruppo = Convert.ToInt32(await q.ExecuteScalarAsync());
        }
        await using (var m = conn.CreateCommand())
        {
            m.CommandText = $"INSERT INTO membri_gruppo(id_gruppo,studente_email) VALUES({_idGruppo},'c1@studio.it')";
            await m.ExecuteNonQueryAsync();
        }
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

    private record ConsegnaGruppoDtoTest(int idGruppo, string stato, object? @abstract, object? progetto);

    [Fact]
    public async Task Abstract_To_Grade_Flow()
    {
        var stud = Client("c1@studio.it", "STUDENTE");
        var doc = Client("prof.c@unibo.it", "DOCENTE");
        var g = _idGruppo;

        Assert.Equal(HttpStatusCode.OK,
            (await stud.PostAsJsonAsync($"/gruppi/{g}/abstract", new { descrizione = "idea" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await doc.PostAsync($"/gruppi/{g}/abstract/approva", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await stud.PostAsJsonAsync($"/gruppi/{g}/progetto", new { fileZip = "p.zip" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await doc.PostAsJsonAsync($"/gruppi/{g}/progetto/valuta", new { valutazione = 28 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await stud.PostAsync($"/gruppi/{g}/progetto/accetta", null)).StatusCode);

        var panoramica = await doc.GetFromJsonAsync<List<ConsegnaGruppoDtoTest>>("/sessioni/CSES0001/consegne");
        Assert.Single(panoramica!);
        Assert.Equal("PROGETTO_VALUTATO", panoramica![0].stato);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var res = await Factory().CreateClient().GetAsync($"/gruppi/{_idGruppo}/abstract");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Studente_Cannot_Grade()
    {
        var res = await Client("c1@studio.it", "STUDENTE")
            .PostAsJsonAsync($"/gruppi/{_idGruppo}/progetto/valuta", new { valutazione = 30 });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }
}
