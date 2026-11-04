using Auth.Data;
using Auth.Logging;
using Auth.Services;
using Microsoft.EntityFrameworkCore;
using UniProjectHub.Shared;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(conn))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("ConnectionStrings:Default must be configured.");
    conn = "server=localhost;port=3306;database=uniprojecthub;user=root;password=uph_dev_mysql_password_change_me";
}
builder.Services.AddDbContext<AuthDbContext>(o =>
    o.UseMySql(conn, ServerVersion.AutoDetect(conn)));

var secret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(secret))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Jwt:Secret must be configured.");
    secret = "uph_dev_jwt_secret_key_minimum_32_chars_change_me!!";
}

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddScoped<IEmailPolicy, EmailPolicy>();
builder.Services.AddScoped<IEventoLogger, EfEventoLogger>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddSingleton<IJwtService>(_ => new JwtService(
    secret,
    builder.Configuration["Jwt:Issuer"] ?? "uph",
    builder.Configuration["Jwt:Audience"] ?? "uph"));

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(HealthResponse.Ok("auth")));
app.MapControllers();

app.Run();

public partial class Program { }
