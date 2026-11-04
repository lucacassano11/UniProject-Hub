using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gruppi.Dtos;
using Gruppi.Services;

namespace Gruppi.Controllers;

[ApiController]
[Route("")]
[Authorize(Roles = "STUDENTE")]
public class GruppiController(
    IGruppoService gruppi, IInvitoService inviti, IAccessoService accessi, IDimensioneService dimensioni)
    : ControllerBase
{
    private string Email => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "";

    // --- gruppi ---
    [HttpPost("gruppi")]
    public async Task<IActionResult> Create(CreateGruppoRequest request)
    {
        try { return Ok(await gruppi.CreateAsync(Email, request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("gruppi/miei")]
    public async Task<IActionResult> Miei() => Ok(await gruppi.ListMieiAsync(Email));

    [HttpGet("sessioni/{idSessione}/gruppi")]
    public async Task<IActionResult> PerSessione(string idSessione) =>
        Ok(await gruppi.ListBySessioneAsync(idSessione));

    [HttpGet("sessioni/{idSessione}/esiste")]
    public async Task<IActionResult> Esiste(string idSessione) =>
        Ok(new { esiste = await gruppi.SessioneEsisteAsync(idSessione) });

    [HttpGet("gruppi/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var g = await gruppi.GetAsync(id);
        return g is null ? NotFound() : Ok(g);
    }

    [HttpPost("gruppi/{id:int}/abbandona")]
    public async Task<IActionResult> Leave(int id)
    {
        var msg = await gruppi.LeaveAsync(Email, id);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    // --- inviti ---
    [HttpPost("gruppi/{id:int}/inviti")]
    public async Task<IActionResult> Invita(int id, InvitoRequest request)
    {
        var dto = await inviti.InviteAsync(Email, id, request);
        return dto is null ? BadRequest(new { error = "Invito non consentito" }) : Ok(dto);
    }

    [HttpGet("inviti/miei")]
    public async Task<IActionResult> InvitiMiei() => Ok(await inviti.ListMieiAsync(Email));

    [HttpPost("inviti/{idRichiesta:int}/accetta")]
    public async Task<IActionResult> AccettaInvito(int idRichiesta)
    {
        var msg = await inviti.AcceptAsync(Email, idRichiesta);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [HttpPost("inviti/{idRichiesta:int}/rifiuta")]
    public async Task<IActionResult> RifiutaInvito(int idRichiesta)
    {
        var msg = await inviti.RejectAsync(Email, idRichiesta);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    // --- richieste di accesso ---
    [HttpPost("gruppi/{id:int}/richieste-accesso")]
    public async Task<IActionResult> ChiediAccesso(int id)
    {
        var dto = await accessi.RequestAsync(Email, id);
        return dto is null ? BadRequest(new { error = "Richiesta non consentita" }) : Ok(dto);
    }

    [HttpGet("gruppi/{id:int}/richieste-accesso")]
    public async Task<IActionResult> AccessiGruppo(int id) =>
        Ok(await accessi.ListPerGruppoAsync(Email, id));

    [HttpPost("richieste-accesso/{idRichiesta:int}/accetta")]
    public async Task<IActionResult> AccettaAccesso(int idRichiesta)
    {
        var msg = await accessi.AcceptAsync(Email, idRichiesta);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [HttpPost("richieste-accesso/{idRichiesta:int}/rifiuta")]
    public async Task<IActionResult> RifiutaAccesso(int idRichiesta)
    {
        var msg = await accessi.RejectAsync(Email, idRichiesta);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    // --- richiesta dimensione ---
    [HttpPost("gruppi/{id:int}/richieste-dimensione")]
    public async Task<IActionResult> ChiediDimensione(int id, DimensioneRequest request)
    {
        var dto = await dimensioni.RequestAsync(Email, id, request);
        return dto is null ? BadRequest(new { error = "Richiesta non consentita" }) : Ok(dto);
    }

    [HttpGet("gruppi/{id:int}/richieste-dimensione")]
    public async Task<IActionResult> DimensioniGruppo(int id) =>
        Ok(await dimensioni.ListPerGruppoAsync(Email, id));
}
