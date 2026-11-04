using Gruppi.Dtos;

namespace Gruppi.Services;

public interface IAccessoService
{
    Task<AccessoDto?> RequestAsync(string studenteEmail, int idGruppo);
    Task<List<AccessoDto>> ListPerGruppoAsync(string capogruppoEmail, int idGruppo);
    Task<string> AcceptAsync(string capogruppoEmail, int idRichiesta);
    Task<string> RejectAsync(string capogruppoEmail, int idRichiesta);
}
