using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sessioni.Dtos;
using Sessioni.Services;

namespace Sessioni.Controllers;

[ApiController]
[Route("")]
[Authorize(Roles = "DOCENTE")]
public class SessioniController(ISessionService sessions, IAppelloService appelli) : ControllerBase
{
    private string Email => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "";

    [HttpGet("sessioni")]
    public async Task<IActionResult> List() => Ok(await sessions.ListAsync(Email));

    [HttpPost("sessioni")]
    public async Task<IActionResult> Create(CreateSessionRequest request) =>
        Ok(await sessions.CreateAsync(Email, request));

    [HttpGet("sessioni/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var s = await sessions.GetAsync(Email, id);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPut("sessioni/{id}/inizio-abstract")]
    public async Task<IActionResult> SetInizio(string id, InizioAbstractRequest request) =>
        await sessions.SetInizioAbstractAsync(Email, id, request) ? NoContent() : NotFound();

    [HttpDelete("sessioni/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var msg = await sessions.DeleteAsync(Email, id);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [HttpGet("sessioni/{id}/appelli")]
    public async Task<IActionResult> ListAppelli(string id) => Ok(await appelli.ListAsync(Email, id));

    [HttpPost("sessioni/{id}/appelli")]
    public async Task<IActionResult> AddAppello(string id, CreateAppelloRequest request)
    {
        var a = await appelli.AddAsync(Email, id, request);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpDelete("appelli/{idAppello:int}")]
    public async Task<IActionResult> DeleteAppello(int idAppello)
    {
        var msg = await appelli.DeleteAsync(Email, idAppello);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }
}
