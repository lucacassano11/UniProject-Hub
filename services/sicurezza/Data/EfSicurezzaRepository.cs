using Microsoft.EntityFrameworkCore;
using Sicurezza.Models;

namespace Sicurezza.Data;

public class EfSicurezzaRepository(SicurezzaDbContext db) : ISicurezzaRepository
{
    public Task<List<UtenteRef>> ListUtentiAsync() =>
        db.Utenti.OrderBy(u => u.Email).ToListAsync();

    public Task<UtenteRef?> FindUtenteAsync(string email) =>
        db.Utenti.FirstOrDefaultAsync(u => u.Email == email);

    public async Task UpdateUtenteAsync(UtenteRef utente)
    {
        db.Utenti.Update(utente);
        await db.SaveChangesAsync();
    }
}
