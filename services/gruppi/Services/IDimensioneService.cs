using Gruppi.Dtos;

namespace Gruppi.Services;

public interface IDimensioneService
{
    Task<DimensioneDto?> RequestAsync(string capogruppoEmail, int idGruppo, DimensioneRequest request);
    Task<List<DimensioneDto>> ListPerGruppoAsync(string capogruppoEmail, int idGruppo);
}
