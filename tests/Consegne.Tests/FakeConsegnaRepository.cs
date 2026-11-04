using Consegne.Data;
using Consegne.Models;

public class FakeConsegnaRepository : IConsegnaRepository
{
    public readonly List<GruppoRef> Gruppi = new();
    public readonly List<Abstract> Abstracts = new();
    public readonly List<Progetto> Progetti = new();
    public readonly Dictionary<string, string> DocentiPerSessione = new();
    public readonly HashSet<(int, string)> Membri = new();
    private int _nextAbstractId = 1;
    private int _nextProgettoId = 1;

    public Task<GruppoRef?> FindGruppoAsync(int id) => Task.FromResult(Gruppi.FirstOrDefault(g => g.IdGruppo == id));
    public Task UpdateGruppoStatoAsync(GruppoRef g) => Task.CompletedTask; // riferimento già in lista
    public Task<List<GruppoRef>> ListGruppiBySessioneAsync(string id) =>
        Task.FromResult(Gruppi.Where(g => g.IdSessione == id).ToList());
    public Task<string?> DocenteDellaSessioneAsync(string id) =>
        Task.FromResult(DocentiPerSessione.TryGetValue(id, out var d) ? d : null);
    public Task<bool> IsMembroAsync(int id, string email) => Task.FromResult(Membri.Contains((id, email)));

    public Task<Abstract?> FindAbstractByGruppoAsync(int id) =>
        Task.FromResult(Abstracts.FirstOrDefault(a => a.IdGruppo == id));
    public Task AddAbstractAsync(Abstract a) { a.IdAbstract = _nextAbstractId++; Abstracts.Add(a); return Task.CompletedTask; }
    public Task UpdateAbstractAsync(Abstract a) => Task.CompletedTask;

    public Task<Progetto?> FindProgettoByGruppoAsync(int id) =>
        Task.FromResult(Progetti.FirstOrDefault(p => p.IdGruppo == id));
    public Task AddProgettoAsync(Progetto p) { p.IdProgetto = _nextProgettoId++; Progetti.Add(p); return Task.CompletedTask; }
    public Task UpdateProgettoAsync(Progetto p) => Task.CompletedTask;
}
