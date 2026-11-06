using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Log.Services;

namespace Log.Controllers;

[ApiController]
[Route("")]
[Authorize(Roles = "GESTORE_SICUREZZA")]
public class LogController(ILogService log) : ControllerBase
{
    [HttpGet("eventi")]
    public async Task<IActionResult> List([FromQuery] string? tipo, [FromQuery] int limit = 100) =>
        Ok(await log.ListAsync(tipo, limit));
}
