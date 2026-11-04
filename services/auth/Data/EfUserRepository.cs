using Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class EfUserRepository(AuthDbContext db) : IUserRepository
{
    public Task<Utente?> FindByEmailAsync(string email) =>
        db.Utenti.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> ExistsAsync(string email) =>
        db.Utenti.AnyAsync(u => u.Email == email);

    public Task<bool> MatricolaExistsAsync(string matricola) =>
        db.Studenti.AnyAsync(s => s.Matricola == matricola);

    public async Task AddStudenteAsync(Utente utente, string matricola)
    {
        db.Utenti.Add(utente);
        db.Studenti.Add(new Studente { Email = utente.Email, Matricola = matricola });
        await db.SaveChangesAsync();
    }

    public async Task AddDocenteAsync(Utente utente)
    {
        db.Utenti.Add(utente);
        db.Docenti.Add(new Docente { Email = utente.Email });
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Utente utente)
    {
        db.Utenti.Update(utente);
        await db.SaveChangesAsync();
    }
}
