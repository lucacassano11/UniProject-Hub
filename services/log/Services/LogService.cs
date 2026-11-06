using Log.Data;
using Log.Dtos;

namespace Log.Services;

public class LogService(ILogRepository repo) : ILogService
{
    public async Task<List<EventoDto>> ListAsync(string? tipo, int limit)
    {
        if (limit <= 0 || limit > 500) limit = 100;
        var list = await repo.ListAsync(tipo, limit);
        return list.Select(e => new EventoDto(e.IdEvento, e.Tipo, e.AttoreEmail, e.Dettaglio, e.CreatedAt)).ToList();
    }
}
