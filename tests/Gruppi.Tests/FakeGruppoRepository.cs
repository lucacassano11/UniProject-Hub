using Gruppi.Data;
using Gruppi.Models;

public class FakeGruppoRepository : IGruppoRepository
{
    public readonly List<Gruppo> Gruppi = new();
    public readonly List<MembroGruppo> Membri = new();
    public readonly List<Richiesta> Richieste = new();
    public readonly List<Invito> Inviti = new();
    public readonly List<RichiestaAccesso> Accessi = new();
    public readonly List<RichiestaDimensione> Dimensioni = new();
    public readonly HashSet<string> Sessioni = new();
    public readonly Dictionary<string, string> DocentiPerSessione = new();
    public readonly HashSet<string> Studenti = new();
    private int _nextGruppoId = 1;
    private int _nextRichiestaId = 1;

    public Task<bool> SessioneExistsAsync(string id) => Task.FromResult(Sessioni.Contains(id));
    public Task<bool> StudenteExistsAsync(string email) => Task.FromResult(Studenti.Contains(email));
    public Task<string?> DocenteDellaSessioneAsync(string id) =>
        Task.FromResult(DocentiPerSessione.TryGetValue(id, out var d) ? d : null);

    public Task<Gruppo?> FindAsync(int id) => Task.FromResult(Gruppi.FirstOrDefault(g => g.IdGruppo == id));
    public Task<List<Gruppo>> ListBySessioneAsync(string id) =>
        Task.FromResult(Gruppi.Where(g => g.IdSessione == id).ToList());
    public Task<Gruppo?> FindGruppoDiStudenteInSessioneAsync(string email, string id)
    {
        var gIds = Membri.Where(m => m.StudenteEmail == email).Select(m => m.IdGruppo).ToHashSet();
        return Task.FromResult(Gruppi.FirstOrDefault(g => g.IdSessione == id && gIds.Contains(g.IdGruppo)));
    }
    public Task AddAsync(Gruppo g) { g.IdGruppo = _nextGruppoId++; Gruppi.Add(g); return Task.CompletedTask; }
    public Task UpdateAsync(Gruppo g) => Task.CompletedTask;
    public Task DeleteAsync(Gruppo g) { Gruppi.Remove(g); return Task.CompletedTask; }

    public Task<List<MembroGruppo>> ListMembriAsync(int id) =>
        Task.FromResult(Membri.Where(m => m.IdGruppo == id).OrderBy(m => m.DataIngresso).ToList());
    public Task<bool> IsMembroAsync(int id, string email) =>
        Task.FromResult(Membri.Any(m => m.IdGruppo == id && m.StudenteEmail == email));
    public Task<int> CountMembriAsync(int id) => Task.FromResult(Membri.Count(m => m.IdGruppo == id));
    public Task AddMembroAsync(MembroGruppo m) { Membri.Add(m); return Task.CompletedTask; }
    public Task RemoveMembroAsync(MembroGruppo m) { Membri.RemoveAll(x => x.IdGruppo == m.IdGruppo && x.StudenteEmail == m.StudenteEmail); return Task.CompletedTask; }
    public Task<List<MembroGruppo>> ListGruppiDiStudenteAsync(string email) =>
        Task.FromResult(Membri.Where(m => m.StudenteEmail == email).ToList());

    public Task<Richiesta?> FindRichiestaAsync(int id) => Task.FromResult(Richieste.FirstOrDefault(r => r.IdRichiesta == id));
    public Task UpdateRichiestaAsync(Richiesta r) => Task.CompletedTask;

    public Task<int> AddInvitoAsync(Richiesta r, Invito i)
    { r.IdRichiesta = _nextRichiestaId++; Richieste.Add(r); i.IdRichiesta = r.IdRichiesta; Inviti.Add(i); return Task.FromResult(r.IdRichiesta); }
    public Task<Invito?> FindInvitoAsync(int id) => Task.FromResult(Inviti.FirstOrDefault(i => i.IdRichiesta == id));
    public Task<List<(Richiesta, Invito)>> ListInvitiPerStudenteAsync(string email, bool soloPendenti) =>
        Task.FromResult(Inviti.Where(i => i.StudenteEmail == email)
            .Select(i => (Richieste.First(r => r.IdRichiesta == i.IdRichiesta), i))
            .Where(t => !soloPendenti || t.Item1.Esito == null).ToList());

    public Task<int> AddAccessoAsync(Richiesta r, RichiestaAccesso a)
    { r.IdRichiesta = _nextRichiestaId++; Richieste.Add(r); a.IdRichiesta = r.IdRichiesta; Accessi.Add(a); return Task.FromResult(r.IdRichiesta); }
    public Task<RichiestaAccesso?> FindAccessoAsync(int id) => Task.FromResult(Accessi.FirstOrDefault(a => a.IdRichiesta == id));
    public Task<List<(Richiesta, RichiestaAccesso)>> ListAccessiPerGruppoAsync(int idGruppo, bool soloPendenti) =>
        Task.FromResult(Accessi.Where(a => a.IdGruppo == idGruppo)
            .Select(a => (Richieste.First(r => r.IdRichiesta == a.IdRichiesta), a))
            .Where(t => !soloPendenti || t.Item1.Esito == null).ToList());

    public Task<int> AddDimensioneAsync(Richiesta r, RichiestaDimensione d)
    { r.IdRichiesta = _nextRichiestaId++; Richieste.Add(r); d.IdRichiesta = r.IdRichiesta; Dimensioni.Add(d); return Task.FromResult(r.IdRichiesta); }
    public Task<List<(Richiesta, RichiestaDimensione)>> ListDimensioniPerGruppoAsync(int idGruppo) =>
        Task.FromResult(Dimensioni.Where(d => d.IdGruppo == idGruppo)
            .Select(d => (Richieste.First(r => r.IdRichiesta == d.IdRichiesta), d)).ToList());

    public Task DeleteRichiesteDiGruppoAsync(int idGruppo)
    {
        var ids = Inviti.Where(i => i.IdGruppo == idGruppo).Select(i => i.IdRichiesta)
            .Concat(Accessi.Where(a => a.IdGruppo == idGruppo).Select(a => a.IdRichiesta))
            .Concat(Dimensioni.Where(d => d.IdGruppo == idGruppo).Select(d => d.IdRichiesta)).ToHashSet();
        Inviti.RemoveAll(i => i.IdGruppo == idGruppo);
        Accessi.RemoveAll(a => a.IdGruppo == idGruppo);
        Dimensioni.RemoveAll(d => d.IdGruppo == idGruppo);
        Richieste.RemoveAll(r => ids.Contains(r.IdRichiesta));
        return Task.CompletedTask;
    }
}
