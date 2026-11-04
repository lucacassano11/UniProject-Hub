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

public class GruppiEndpointsIntegrationTests : IAsyncLifetime
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
        // seed: docente + sessione + 2 studenti
        await using var seed = conn.CreateCommand();
        seed.CommandText =
            "INSERT INTO utenti(email,password_hash,nome,cognome,ruolo) VALUES" +
            "('prof.g@unibo.it','x','P','G','DOCENTE')," +
            "('s1@studio.it','x','S','Uno','STUDENTE')," +
            "('s2@studio.it','x','S','Due','STUDENTE');" +
            "INSERT INTO docenti(email) VALUES('prof.g@unibo.it');" +
            "INSERT INTO studenti(email,matricola) VALUES('s1@studio.it','9001'),('s2@studio.it','9002');" +
            "INSERT INTO sessioni(id_sessione,voto_massimo,data_creazione_sessione,docente_email) " +
            "VALUES('GSES0001',30,'2026-03-01','prof.g@unibo.it');";
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

    private record GruppoDtoTest(int idGruppo, string idSessione, int dimensione, string stato, string? capogruppoEmail, int numeroMembri);
    private record InvitoDtoTest(int idRichiesta, int idGruppo, string studenteEmail, bool? esito);

    [Fact]
    public async Task Create_Invite_Accept_Leave_Flow()
    {
        var capo = Client("s1@studio.it", "STUDENTE");
        var create = await capo.PostAsJsonAsync("/gruppi", new { idSessione = "GSES0001" });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var g = await create.Content.ReadFromJsonAsync<GruppoDtoTest>();
        Assert.Equal("s1@studio.it", g!.capogruppoEmail);

        var inv = await capo.PostAsJsonAsync($"/gruppi/{g.idGruppo}/inviti", new { studenteEmail = "s2@studio.it" });
        Assert.Equal(HttpStatusCode.OK, inv.StatusCode);
        var invito = await inv.Content.ReadFromJsonAsync<InvitoDtoTest>();

        var membro = Client("s2@studio.it", "STUDENTE");
        var accept = await membro.PostAsync($"/inviti/{invito!.idRichiesta}/accetta", null);
        Assert.Equal(HttpStatusCode.NoContent, accept.StatusCode);

        var detail = await capo.GetFromJsonAsync<GruppoDtoTest>($"/gruppi/{g.idGruppo}");
        Assert.Equal(2, detail!.numeroMembri);

        var leave = await membro.PostAsync($"/gruppi/{g.idGruppo}/abbandona", null);
        Assert.Equal(HttpStatusCode.NoContent, leave.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var res = await Factory().CreateClient().GetAsync("/gruppi/miei");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Docente_Is_Forbidden()
    {
        var res = await Client("prof.g@unibo.it", "DOCENTE").GetAsync("/gruppi/miei");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }
}
