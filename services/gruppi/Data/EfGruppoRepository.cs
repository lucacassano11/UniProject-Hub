using Microsoft.EntityFrameworkCore;
using Gruppi.Models;

namespace Gruppi.Data;

public class EfGruppoRepository(GruppiDbContext db) : IGruppoRepository
{
    public async Task<bool> SessioneExistsAsync(string idSessione)
    {
        var n = await db.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM sessioni WHERE id_sessione = {idSessione}")
            .FirstAsync();
        return n > 0;
    }

    public async Task<bool> StudenteExistsAsync(string email)
    {
        var n = await db.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM studenti WHERE email = {email}")
            .FirstAsync();
        return n > 0;
    }

    public async Task<string?> DocenteDellaSessioneAsync(string idSessione)
    {
        var list = await db.Database
            .SqlQuery<string>($"SELECT docente_email AS Value FROM sessioni WHERE id_sessione = {idSessione}")
            .ToListAsync();
        return list.FirstOrDefault();
    }

    public Task<Gruppo?> FindAsync(int idGruppo) =>
        db.Gruppi.FirstOrDefaultAsync(g => g.IdGruppo == idGruppo);

    public Task<List<Gruppo>> ListBySessioneAsync(string idSessione) =>
        db.Gruppi.Where(g => g.IdSessione == idSessione).ToListAsync();

    public async Task<Gruppo?> FindGruppoDiStudenteInSessioneAsync(string email, string idSessione)
    {
        var query = from m in db.Membri
                    join g in db.Gruppi on m.IdGruppo equals g.IdGruppo
                    where m.StudenteEmail == email && g.IdSessione == idSessione
                    select g;
        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(Gruppo g) { db.Gruppi.Add(g); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Gruppo g) { db.Gruppi.Update(g); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(Gruppo g) { db.Gruppi.Remove(g); await db.SaveChangesAsync(); }

    public Task<List<MembroGruppo>> ListMembriAsync(int idGruppo) =>
        db.Membri.Where(m => m.IdGruppo == idGruppo).OrderBy(m => m.DataIngresso).ToListAsync();
    public Task<bool> IsMembroAsync(int idGruppo, string email) =>
        db.Membri.AnyAsync(m => m.IdGruppo == idGruppo && m.StudenteEmail == email);
    public Task<int> CountMembriAsync(int idGruppo) =>
        db.Membri.CountAsync(m => m.IdGruppo == idGruppo);
    public async Task AddMembroAsync(MembroGruppo m) { db.Membri.Add(m); await db.SaveChangesAsync(); }
    public async Task RemoveMembroAsync(MembroGruppo m)
    {
        var existing = await db.Membri.FirstOrDefaultAsync(x => x.IdGruppo == m.IdGruppo && x.StudenteEmail == m.StudenteEmail);
        if (existing is not null) { db.Membri.Remove(existing); await db.SaveChangesAsync(); }
    }
    public Task<List<MembroGruppo>> ListGruppiDiStudenteAsync(string email) =>
        db.Membri.Where(m => m.StudenteEmail == email).ToListAsync();

    public Task<Richiesta?> FindRichiestaAsync(int idRichiesta) =>
        db.Richieste.FirstOrDefaultAsync(r => r.IdRichiesta == idRichiesta);
    public async Task UpdateRichiestaAsync(Richiesta r) { db.Richieste.Update(r); await db.SaveChangesAsync(); }

    public async Task<int> AddInvitoAsync(Richiesta r, Invito i)
    {
        db.Richieste.Add(r);
        await db.SaveChangesAsync();   // genera id_richiesta
        i.IdRichiesta = r.IdRichiesta;
        db.Inviti.Add(i);
        await db.SaveChangesAsync();
        return r.IdRichiesta;
    }
    public Task<Invito?> FindInvitoAsync(int idRichiesta) =>
        db.Inviti.FirstOrDefaultAsync(i => i.IdRichiesta == idRichiesta);
    public async Task<List<(Richiesta, Invito)>> ListInvitiPerStudenteAsync(string email, bool soloPendenti)
    {
        var q = from i in db.Inviti
                join r in db.Richieste on i.IdRichiesta equals r.IdRichiesta
                where i.StudenteEmail == email && (!soloPendenti || r.Esito == null)
                select new { r, i };
        return (await q.ToListAsync()).Select(x => (x.r, x.i)).ToList();
    }

    public async Task<int> AddAccessoAsync(Richiesta r, RichiestaAccesso a)
    {
        db.Richieste.Add(r);
        await db.SaveChangesAsync();
        a.IdRichiesta = r.IdRichiesta;
        db.RichiesteAccesso.Add(a);
        await db.SaveChangesAsync();
        return r.IdRichiesta;
    }
    public Task<RichiestaAccesso?> FindAccessoAsync(int idRichiesta) =>
        db.RichiesteAccesso.FirstOrDefaultAsync(a => a.IdRichiesta == idRichiesta);
    public async Task<List<(Richiesta, RichiestaAccesso)>> ListAccessiPerGruppoAsync(int idGruppo, bool soloPendenti)
    {
        var q = from a in db.RichiesteAccesso
                join r in db.Richieste on a.IdRichiesta equals r.IdRichiesta
                where a.IdGruppo == idGruppo && (!soloPendenti || r.Esito == null)
                select new { r, a };
        return (await q.ToListAsync()).Select(x => (x.r, x.a)).ToList();
    }

    public async Task<int> AddDimensioneAsync(Richiesta r, RichiestaDimensione d)
    {
        db.Richieste.Add(r);
        await db.SaveChangesAsync();
        d.IdRichiesta = r.IdRichiesta;
        db.RichiesteDimensione.Add(d);
        await db.SaveChangesAsync();
        return r.IdRichiesta;
    }
    public async Task<List<(Richiesta, RichiestaDimensione)>> ListDimensioniPerGruppoAsync(int idGruppo)
    {
        var q = from d in db.RichiesteDimensione
                join r in db.Richieste on d.IdRichiesta equals r.IdRichiesta
                where d.IdGruppo == idGruppo
                select new { r, d };
        return (await q.ToListAsync()).Select(x => (x.r, x.d)).ToList();
    }

    public async Task DeleteRichiesteDiGruppoAsync(int idGruppo)
    {
        var inviti = await db.Inviti.Where(i => i.IdGruppo == idGruppo).ToListAsync();
        var accessi = await db.RichiesteAccesso.Where(a => a.IdGruppo == idGruppo).ToListAsync();
        var dim = await db.RichiesteDimensione.Where(d => d.IdGruppo == idGruppo).ToListAsync();
        var ids = inviti.Select(i => i.IdRichiesta)
            .Concat(accessi.Select(a => a.IdRichiesta))
            .Concat(dim.Select(d => d.IdRichiesta)).ToList();
        db.Inviti.RemoveRange(inviti);
        db.RichiesteAccesso.RemoveRange(accessi);
        db.RichiesteDimensione.RemoveRange(dim);
        await db.SaveChangesAsync();
        var basi = await db.Richieste.Where(r => ids.Contains(r.IdRichiesta)).ToListAsync();
        db.Richieste.RemoveRange(basi);
        await db.SaveChangesAsync();
    }
}
