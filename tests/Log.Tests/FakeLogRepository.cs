using Log.Data;
using Log.Models;

public class FakeLogRepository : ILogRepository
{
    public readonly List<Evento> Eventi = new();
    private int _nextId = 1;

    public Evento Seed(string tipo, string? attore, string dettaglio, DateTime? when = null)
    {
        var e = new Evento
        {
            IdEvento = _nextId++, Tipo = tipo, AttoreEmail = attore,
            Dettaglio = dettaglio, CreatedAt = when ?? DateTime.UtcNow,
        };
        Eventi.Add(e);
        return e;
    }

    public Task<List<Evento>> ListAsync(string? tipo, int limit) =>
        Task.FromResult(Eventi
            .Where(e => tipo == null || e.Tipo == tipo)
            .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.IdEvento)
            .Take(limit).ToList());
}
