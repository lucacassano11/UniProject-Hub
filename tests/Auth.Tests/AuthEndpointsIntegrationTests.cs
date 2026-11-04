using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using Xunit;

public class AuthEndpointsIntegrationTests : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder()
        .WithImage("mysql:8")
        .WithDatabase("uniprojecthub")
        .Build();

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();
        var sql = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "init.sql"));
        await using var conn = new MySqlConnector.MySqlConnection(_mysql.GetConnectionString());
        await conn.OpenAsync();
        foreach (var raw in sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var stmt = string.Join('\n',
                raw.Split('\n').Where(l => !l.TrimStart().StartsWith("--")))
                .Trim();
            if (stmt.Length == 0) continue;
            // Skip CREATE DATABASE / USE statements — the container already created the DB
            // and the Testcontainers-generated user may lack CREATE DATABASE privilege.
            if (stmt.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase)) continue;
            if (stmt.StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) continue;
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public Task DisposeAsync() => _mysql.DisposeAsync().AsTask();

    private WebApplicationFactory<Program> Factory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Default", _mysql.GetConnectionString());
            b.UseSetting("Jwt:Secret", "test-secret-at-least-32-chars-long-xx");
        });

    [Fact]
    public async Task Register_Then_Login_Works()
    {
        var client = Factory().CreateClient();

        var reg = await client.PostAsJsonAsync("/register",
            new { email = "nuovo.studente@studio.unibo.it", password = "Abcdef1!", ruolo = "STUDENTE" });
        Assert.Equal(HttpStatusCode.OK, reg.StatusCode);

        var login = await client.PostAsJsonAsync("/login",
            new { email = "nuovo.studente@studio.unibo.it", password = "Abcdef1!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await login.Content.ReadFromJsonAsync<Auth.Dtos.AuthResponse>();
        Assert.False(string.IsNullOrEmpty(body!.Token));
    }

    [Fact]
    public async Task Login_Wrong_Password_Returns_401()
    {
        var client = Factory().CreateClient();
        await client.PostAsJsonAsync("/register",
            new { email = "tizio@unibo.it", password = "Abcdef1!", ruolo = "DOCENTE" });

        var login = await client.PostAsJsonAsync("/login",
            new { email = "tizio@unibo.it", password = "Sbagliata9!" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
