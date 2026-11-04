using Auth.Models;

namespace Auth.Data;

public interface IUserRepository
{
    Task<Utente?> FindByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
    Task<bool> MatricolaExistsAsync(string matricola);
    Task AddStudenteAsync(Utente utente, string matricola);
    Task AddDocenteAsync(Utente utente);
    Task UpdateAsync(Utente utente);
}
