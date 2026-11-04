using Sessioni.Models;

namespace Sessioni.Data;

public interface ISessionRepository
{
    Task<bool> ExistsAsync(string idSessione);
    Task<Sessione?> FindAsync(string idSessione);
    Task<List<Sessione>> ListByDocenteAsync(string docenteEmail);
    Task AddAsync(Sessione sessione);
    Task UpdateAsync(Sessione sessione);
    Task DeleteAsync(Sessione sessione);
    Task<int> CountGruppiAsync(string idSessione);

    Task<List<Appello>> ListAppelliAsync(string idSessione);
    Task<Appello?> FindAppelloAsync(int idAppello);
    Task AddAppelloAsync(Appello appello);
    Task DeleteAppelloAsync(Appello appello);

    // Per il job di pulizia
    Task<List<Sessione>> ListCreatedBeforeAsync(DateOnly soglia);
}
