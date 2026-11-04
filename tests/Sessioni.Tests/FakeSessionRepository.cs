using Sessioni.Data;
using Sessioni.Models;

public class FakeSessionRepository : ISessionRepository
{
    public readonly List<Sessione> Sessioni = new();
    public readonly List<Appello> Appelli = new();
    public readonly Dictionary<string, int> GruppiCount = new(); // idSessione -> n gruppi
    private int _nextAppelloId = 1;

    public Task<bool> ExistsAsync(string id) => Task.FromResult(Sessioni.Any(s => s.IdSessione == id));
    public Task<Sessione?> FindAsync(string id) => Task.FromResult(Sessioni.FirstOrDefault(s => s.IdSessione == id));
    public Task<List<Sessione>> ListByDocenteAsync(string email) =>
        Task.FromResult(Sessioni.Where(s => s.DocenteEmail == email).ToList());
    public Task AddAsync(Sessione s) { Sessioni.Add(s); return Task.CompletedTask; }
    public Task UpdateAsync(Sessione s) => Task.CompletedTask;
    public Task DeleteAsync(Sessione s) { Sessioni.Remove(s); return Task.CompletedTask; }
    public Task<int> CountGruppiAsync(string id) =>
        Task.FromResult(GruppiCount.TryGetValue(id, out var n) ? n : 0);

    public Task<List<Appello>> ListAppelliAsync(string id) =>
        Task.FromResult(Appelli.Where(a => a.IdSessione == id).ToList());
    public Task<Appello?> FindAppelloAsync(int id) =>
        Task.FromResult(Appelli.FirstOrDefault(a => a.IdAppello == id));
    public Task AddAppelloAsync(Appello a) { a.IdAppello = _nextAppelloId++; Appelli.Add(a); return Task.CompletedTask; }
    public Task DeleteAppelloAsync(Appello a) { Appelli.Remove(a); return Task.CompletedTask; }

    public Task<List<Sessione>> ListCreatedBeforeAsync(DateOnly soglia) =>
        Task.FromResult(Sessioni.Where(s => s.DataCreazioneSessione < soglia).ToList());
}
