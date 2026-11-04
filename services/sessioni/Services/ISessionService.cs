using Sessioni.Dtos;

namespace Sessioni.Services;

public interface ISessionService
{
    Task<SessionDto> CreateAsync(string docenteEmail, CreateSessionRequest request);
    Task<List<SessionDto>> ListAsync(string docenteEmail);
    Task<SessionDto?> GetAsync(string docenteEmail, string idSessione);
    Task<bool> SetInizioAbstractAsync(string docenteEmail, string idSessione, InizioAbstractRequest request);
    Task<string> DeleteAsync(string docenteEmail, string idSessione);
}
