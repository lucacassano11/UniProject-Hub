using Log.Dtos;

namespace Log.Services;

public interface ILogService
{
    Task<List<EventoDto>> ListAsync(string? tipo, int limit);
}
