using Microsoft.EntityFrameworkCore;
using Consegne.Models;

namespace Consegne.Data;

public class EfConsegnaRepository(ConsegneDbContext db) : IConsegnaRepository
{
    public Task<GruppoRef?> FindGruppoAsync(int idGruppo) =>
        db.Gruppi.FirstOrDefaultAsync(g => g.IdGruppo == idGruppo);

    public async Task UpdateGruppoStatoAsync(GruppoRef gruppo)
    {
        db.Gruppi.Update(gruppo);
        await db.SaveChangesAsync();
    }

    public Task<List<GruppoRef>> ListGruppiBySessioneAsync(string idSessione) =>
        db.Gruppi.Where(g => g.IdSessione == idSessione).ToListAsync();

    public async Task<string?> DocenteDellaSessioneAsync(string idSessione)
    {
        var list = await db.Database
            .SqlQuery<string>($"SELECT docente_email AS Value FROM sessioni WHERE id_sessione = {idSessione}")
            .ToListAsync();
        return list.FirstOrDefault();
    }

    public async Task<bool> IsMembroAsync(int idGruppo, string email)
    {
        var n = await db.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM membri_gruppo WHERE id_gruppo = {idGruppo} AND studente_email = {email}")
            .FirstAsync();
        return n > 0;
    }

    public Task<Abstract?> FindAbstractByGruppoAsync(int idGruppo) =>
        db.Abstracts.FirstOrDefaultAsync(a => a.IdGruppo == idGruppo);
    public async Task AddAbstractAsync(Abstract a) { db.Abstracts.Add(a); await db.SaveChangesAsync(); }
    public async Task UpdateAbstractAsync(Abstract a) { db.Abstracts.Update(a); await db.SaveChangesAsync(); }

    public Task<Progetto?> FindProgettoByGruppoAsync(int idGruppo) =>
        db.Progetti.FirstOrDefaultAsync(p => p.IdGruppo == idGruppo);
    public async Task AddProgettoAsync(Progetto p) { db.Progetti.Add(p); await db.SaveChangesAsync(); }
    public async Task UpdateProgettoAsync(Progetto p) { db.Progetti.Update(p); await db.SaveChangesAsync(); }
}
