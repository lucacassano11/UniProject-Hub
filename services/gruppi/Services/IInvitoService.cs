using Gruppi.Dtos;

namespace Gruppi.Services;

public interface IInvitoService
{
    Task<InvitoDto?> InviteAsync(string capogruppoEmail, int idGruppo, InvitoRequest request);
    Task<List<InvitoDto>> ListMieiAsync(string studenteEmail);
    Task<string> AcceptAsync(string studenteEmail, int idRichiesta);
    Task<string> RejectAsync(string studenteEmail, int idRichiesta);
}
