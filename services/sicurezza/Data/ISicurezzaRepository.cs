using Sicurezza.Models;

namespace Sicurezza.Data;

public interface ISicurezzaRepository
{
    Task<List<UtenteRef>> ListUtentiAsync();
    Task<UtenteRef?> FindUtenteAsync(string email);
    Task UpdateUtenteAsync(UtenteRef utente);
}
