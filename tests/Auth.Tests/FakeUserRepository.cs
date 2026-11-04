using Auth.Data;
using Auth.Models;

public class FakeUserRepository : IUserRepository
{
    public readonly List<Utente> Utenti = new();
    public readonly Dictionary<string, string> Matricole = new(); // email -> matricola

    public Task<Utente?> FindByEmailAsync(string email) =>
        Task.FromResult(Utenti.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsAsync(string email) =>
        Task.FromResult(Utenti.Any(u => u.Email == email));

    public Task<bool> MatricolaExistsAsync(string matricola) =>
        Task.FromResult(Matricole.ContainsValue(matricola));

    public Task AddStudenteAsync(Utente utente, string matricola)
    {
        Utenti.Add(utente);
        Matricole[utente.Email] = matricola;
        return Task.CompletedTask;
    }

    public Task AddDocenteAsync(Utente utente)
    {
        Utenti.Add(utente);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Utente utente) => Task.CompletedTask;
}
