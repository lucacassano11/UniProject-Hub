using Microsoft.EntityFrameworkCore;
using Log.Models;

namespace Log.Data;

public class EfLogRepository(LogDbContext db) : ILogRepository
{
    public Task<List<Evento>> ListAsync(string? tipo, int limit) =>
        db.Eventi
            .Where(e => tipo == null || e.Tipo == tipo)
            .OrderByDescending(e => e.CreatedAt)
            .ThenByDescending(e => e.IdEvento)
            .Take(limit)
            .ToListAsync();
}
