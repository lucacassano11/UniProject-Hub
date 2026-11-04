using Sicurezza.Data;
using Sicurezza.Models;

public class FakeSicurezzaRepository : ISicurezzaRepository
{
    public readonly List<UtenteRef> Utenti = new();

    public Task<List<UtenteRef>> ListUtentiAsync() =>
        Task.FromResult(Utenti.OrderBy(u => u.Email).ToList());
    public Task<UtenteRef?> FindUtenteAsync(string email) =>
        Task.FromResult(Utenti.FirstOrDefault(u => u.Email == email));
    public Task UpdateUtenteAsync(UtenteRef utente) => Task.CompletedTask;
}
