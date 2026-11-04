using Microsoft.EntityFrameworkCore;
using Sessioni.Models;

namespace Sessioni.Data;

public class EfSessionRepository(SessioniDbContext db) : ISessionRepository
{
    public Task<bool> ExistsAsync(string idSessione) =>
        db.Sessioni.AnyAsync(s => s.IdSessione == idSessione);

    public Task<Sessione?> FindAsync(string idSessione) =>
        db.Sessioni.FirstOrDefaultAsync(s => s.IdSessione == idSessione);

    public Task<List<Sessione>> ListByDocenteAsync(string docenteEmail) =>
        db.Sessioni.Where(s => s.DocenteEmail == docenteEmail).ToListAsync();

    public async Task AddAsync(Sessione sessione) { db.Sessioni.Add(sessione); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Sessione sessione) { db.Sessioni.Update(sessione); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(Sessione sessione) { db.Sessioni.Remove(sessione); await db.SaveChangesAsync(); }

    public Task<int> CountGruppiAsync(string idSessione) =>
        db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM gruppi WHERE id_sessione = {idSessione}")
          .FirstAsync();

    public Task<List<Appello>> ListAppelliAsync(string idSessione) =>
        db.Appelli.Where(a => a.IdSessione == idSessione).ToListAsync();

    public Task<Appello?> FindAppelloAsync(int idAppello) =>
        db.Appelli.FirstOrDefaultAsync(a => a.IdAppello == idAppello);

    public async Task AddAppelloAsync(Appello appello) { db.Appelli.Add(appello); await db.SaveChangesAsync(); }
    public async Task DeleteAppelloAsync(Appello appello) { db.Appelli.Remove(appello); await db.SaveChangesAsync(); }

    public Task<List<Sessione>> ListCreatedBeforeAsync(DateOnly soglia) =>
        db.Sessioni.Where(s => s.DataCreazioneSessione < soglia).ToListAsync();
}
