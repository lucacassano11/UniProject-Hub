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

public class SessioniDbFixture : IAsyncLifetime
{
    private const string Secret = "test-secret-at-least-32-chars-long-xx";
    public string TestSecret => Secret;

    public readonly MySqlContainer Mysql = new MySqlBuilder().WithImage("mysql:8")
        .WithDatabase("uniprojecthub").Build();

    public async Task InitializeAsync()
    {
        await Mysql.StartAsync();
        var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "init.sql"));
        await using var conn = new MySqlConnection(Mysql.GetConnectionString());
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
        // seed docente (FK sessioni.docente_email)
        await using var seed = conn.CreateCommand();
        seed.CommandText =
            "INSERT INTO utenti(email,password_hash,nome,cognome,ruolo) " +
            "VALUES('prof.test@unibo.it','x','Prof','Test','DOCENTE'); " +
            "INSERT INTO docenti(email) VALUES('prof.test@unibo.it');";
        await seed.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Mysql.DisposeAsync().AsTask();

    public HttpClient AuthenticatedClient()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", Mysql.GetConnectionString() + ";SslMode=None");
            b.UseSetting("Jwt:Secret", Secret);
            b.UseSetting("Cleanup:IntervalHours", "9999");
        });
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token("prof.test@unibo.it", "DOCENTE"));
        return client;
    }

    public HttpClient AnonymousClient()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", Mysql.GetConnectionString() + ";SslMode=None");
            b.UseSetting("Jwt:Secret", Secret);
            b.UseSetting("Cleanup:IntervalHours", "9999");
        });
        return factory.CreateClient();
    }

    public static string Token(string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("uph", "uph",
            new[] { new Claim(JwtRegisteredClaimNames.Sub, email), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

public class SessioniEndpointsIntegrationTests : IClassFixture<SessioniDbFixture>
{
    private readonly SessioniDbFixture _fixture;

    public SessioniEndpointsIntegrationTests(SessioniDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_List_AddAppello_Flow()
    {
        var client = _fixture.AuthenticatedClient();

        var create = await client.PostAsJsonAsync("/sessioni", new { votoMassimo = 30 });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var session = await create.Content.ReadFromJsonAsync<SessionDtoTest>();
        Assert.Equal(8, session!.idSessione.Length);

        var list = await client.GetFromJsonAsync<List<SessionDtoTest>>("/sessioni");
        Assert.Single(list!);

        var appello = await client.PostAsJsonAsync($"/sessioni/{session.idSessione}/appelli",
            new { dataOraAppello = "2026-07-01T09:00:00", scadenzaConsegnaProgetto = "2026-06-30" });
        Assert.Equal(HttpStatusCode.OK, appello.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Is_Rejected()
    {
        var client = _fixture.AnonymousClient();
        var res = await client.GetAsync("/sessioni");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    private record SessionDtoTest(string idSessione, int votoMassimo, string dataCreazione, string? inizioInvioAbstract, int numeroAppelli);
}
