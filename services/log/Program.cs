using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Log.Data;
using Log.Services;
using UniProjectHub.Shared;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(conn))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("ConnectionStrings:Default must be configured.");
    conn = "server=localhost;port=3306;database=uniprojecthub;user=root;password=uph_dev_mysql_password_change_me";
}
builder.Services.AddDbContext<LogDbContext>(o =>
    o.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddScoped<ILogRepository, EfLogRepository>();
builder.Services.AddScoped<ILogService, LogService>();

var secret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(secret))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Jwt:Secret must be configured.");
    secret = "uph_dev_jwt_secret_key_minimum_32_chars_change_me!!";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "uph",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "uph",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            RoleClaimType = "role",
            NameClaimType = "sub",
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(HealthResponse.Ok("log")));
app.MapControllers();

app.Run();

public partial class Program { }
