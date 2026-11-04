using Sessioni.Dtos;

namespace Sessioni.Services;

public interface IAppelloService
{
    Task<List<AppelloDto>> ListAsync(string docenteEmail, string idSessione);
    Task<AppelloDto?> AddAsync(string docenteEmail, string idSessione, CreateAppelloRequest request);
    Task<string> DeleteAsync(string docenteEmail, int idAppello);
}
