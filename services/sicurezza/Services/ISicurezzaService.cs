using Sicurezza.Dtos;

namespace Sicurezza.Services;

public interface ISicurezzaService
{
    Task<List<UtenteSicurezzaDto>> ListUtentiAsync();
    Task<bool> BloccaAsync(string gestoreEmail, string targetEmail);
    Task<bool> SbloccaAsync(string gestoreEmail, string targetEmail);
}
