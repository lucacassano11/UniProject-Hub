using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifiche.Services;

namespace Notifiche.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class NotificheController(INotificaService notifiche) : ControllerBase
{
    private string Email => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "";

    [HttpGet("notifiche")]
    public async Task<IActionResult> List([FromQuery] bool soloNonLette = false) =>
        Ok(await notifiche.ListAsync(Email, soloNonLette));

    [HttpGet("notifiche/conteggio")]
    public async Task<IActionResult> Conteggio() =>
        Ok(new { nonLette = await notifiche.CountNonLetteAsync(Email) });

    [HttpPost("notifiche/{id:int}/letta")]
    public async Task<IActionResult> SegnaLetta(int id) =>
        await notifiche.MarkLettaAsync(Email, id) ? NoContent() : NotFound();
}
