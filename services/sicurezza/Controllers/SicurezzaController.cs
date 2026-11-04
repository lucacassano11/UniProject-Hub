using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicurezza.Services;

namespace Sicurezza.Controllers;

[ApiController]
[Route("")]
[Authorize(Roles = "GESTORE_SICUREZZA")]
public class SicurezzaController(ISicurezzaService sicurezza) : ControllerBase
{
    private string Email => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "";

    [HttpGet("utenti")]
    public async Task<IActionResult> Utenti() => Ok(await sicurezza.ListUtentiAsync());

    [HttpPost("utenti/{email}/blocca")]
    public async Task<IActionResult> Blocca(string email) =>
        await sicurezza.BloccaAsync(Email, email) ? NoContent() : NotFound();

    [HttpPost("utenti/{email}/sblocca")]
    public async Task<IActionResult> Sblocca(string email) =>
        await sicurezza.SbloccaAsync(Email, email) ? NoContent() : NotFound();
}
