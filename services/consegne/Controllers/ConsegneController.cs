using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Consegne.Data;
using Consegne.Dtos;
using Consegne.Services;

namespace Consegne.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class ConsegneController(
    IAbstractService abstracts, IProgettoService progetti, IConsegnaRepository repo) : ControllerBase
{
    private string Email => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "";

    // --- abstract ---
    [Authorize(Roles = "STUDENTE")]
    [HttpPost("gruppi/{id:int}/abstract")]
    public async Task<IActionResult> Submit(int id, SubmitAbstractRequest request)
    {
        var dto = await abstracts.SubmitAsync(Email, id, request);
        return dto is null ? BadRequest(new { error = "Consegna non consentita" }) : Ok(dto);
    }

    [HttpGet("gruppi/{id:int}/abstract")]
    public async Task<IActionResult> GetAbstract(int id)
    {
        var dto = await abstracts.GetAsync(Email, id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Roles = "DOCENTE")]
    [HttpPost("gruppi/{id:int}/abstract/approva")]
    public async Task<IActionResult> Approva(int id)
    {
        var msg = await abstracts.ApproveAsync(Email, id);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [Authorize(Roles = "DOCENTE")]
    [HttpPost("gruppi/{id:int}/abstract/rifiuta")]
    public async Task<IActionResult> Rifiuta(int id, RifiutaAbstractRequest request)
    {
        var msg = await abstracts.RejectAsync(Email, id, request);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    // --- progetto ---
    [Authorize(Roles = "STUDENTE")]
    [HttpPost("gruppi/{id:int}/progetto")]
    public async Task<IActionResult> Upload(int id, UploadProgettoRequest request)
    {
        var dto = await progetti.UploadAsync(Email, id, request);
        return dto is null ? BadRequest(new { error = "Upload non consentito" }) : Ok(dto);
    }

    [HttpGet("gruppi/{id:int}/progetto")]
    public async Task<IActionResult> GetProgetto(int id)
    {
        var dto = await progetti.GetAsync(Email, id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Roles = "DOCENTE")]
    [HttpPost("gruppi/{id:int}/progetto/valuta")]
    public async Task<IActionResult> Valuta(int id, ValutaRequest request)
    {
        var msg = await progetti.GradeAsync(Email, id, request);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [Authorize(Roles = "STUDENTE")]
    [HttpPost("gruppi/{id:int}/progetto/accetta")]
    public async Task<IActionResult> AccettaVoto(int id)
    {
        var msg = await progetti.AcceptAsync(Email, id);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    [Authorize(Roles = "STUDENTE")]
    [HttpPost("gruppi/{id:int}/progetto/rifiuta")]
    public async Task<IActionResult> RifiutaVoto(int id)
    {
        var msg = await progetti.RejectAsync(Email, id);
        return msg == "OK" ? NoContent() : BadRequest(new { error = msg });
    }

    // --- panoramica docente ---
    [Authorize(Roles = "DOCENTE")]
    [HttpGet("sessioni/{idSessione}/consegne")]
    public async Task<IActionResult> Panoramica(string idSessione)
    {
        if (Email != await repo.DocenteDellaSessioneAsync(idSessione)) return Forbid();
        var gruppi = await repo.ListGruppiBySessioneAsync(idSessione);
        var result = new List<ConsegnaGruppoDto>();
        foreach (var g in gruppi)
        {
            var a = await abstracts.GetAsync(Email, g.IdGruppo);
            var p = await progetti.GetAsync(Email, g.IdGruppo);
            result.Add(new ConsegnaGruppoDto(g.IdGruppo, g.Stato.ToString(), a, p));
        }
        return Ok(result);
    }
}
