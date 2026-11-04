using Consegne.Dtos;

namespace Consegne.Services;

public interface IProgettoService
{
    Task<ProgettoDto?> UploadAsync(string studenteEmail, int idGruppo, UploadProgettoRequest request);
    Task<ProgettoDto?> GetAsync(string email, int idGruppo);
    Task<string> GradeAsync(string docenteEmail, int idGruppo, ValutaRequest request);
    Task<string> AcceptAsync(string studenteEmail, int idGruppo);
    Task<string> RejectAsync(string studenteEmail, int idGruppo);
}
