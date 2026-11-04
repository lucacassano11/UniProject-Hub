using Consegne.Dtos;

namespace Consegne.Services;

public interface IAbstractService
{
    Task<AbstractDto?> SubmitAsync(string studenteEmail, int idGruppo, SubmitAbstractRequest request);
    Task<AbstractDto?> GetAsync(string email, int idGruppo);
    Task<string> ApproveAsync(string docenteEmail, int idGruppo);
    Task<string> RejectAsync(string docenteEmail, int idGruppo, RifiutaAbstractRequest request);
}
