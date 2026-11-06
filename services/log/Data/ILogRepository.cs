using Log.Models;

namespace Log.Data;

public interface ILogRepository
{
    Task<List<Evento>> ListAsync(string? tipo, int limit);
}
