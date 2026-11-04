using Gruppi.Dtos;

namespace Gruppi.Services;

public interface IGruppoService
{
    Task<GruppoDto> CreateAsync(string studenteEmail, CreateGruppoRequest request);
    Task<List<GruppoDto>> ListMieiAsync(string studenteEmail);
    Task<List<GruppoDto>> ListBySessioneAsync(string idSessione);
    Task<GruppoDto?> GetAsync(int idGruppo);
    Task<string> LeaveAsync(string studenteEmail, int idGruppo);
    Task<bool> SessioneEsisteAsync(string idSessione);
}
