using Auth.Dtos;
using Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("")]
public class AuthController(IRegistrationService registration, ILoginService login) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var res = await registration.RegisterAsync(request);
        return res.Success ? Ok(res.User) : BadRequest(new { error = res.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var res = await login.LoginAsync(request);
        return res.Success ? Ok(res.User) : Unauthorized(new { error = res.Error });
    }

    [HttpPost("recupero-password")]
    public IActionResult RecuperoPassword(RecoveryRequest request)
    {
        // Stub: in produzione invierebbe un'email di reset. Risposta neutra (no user enumeration).
        return Ok(new { message = "Se l'email esiste, riceverai istruzioni per il reset" });
    }
}
