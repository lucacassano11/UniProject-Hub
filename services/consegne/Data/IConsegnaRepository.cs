using Consegne.Models;

namespace Consegne.Data;

public interface IConsegnaRepository
{
    Task<GruppoRef?> FindGruppoAsync(int idGruppo);
    Task UpdateGruppoStatoAsync(GruppoRef gruppo);
    Task<List<GruppoRef>> ListGruppiBySessioneAsync(string idSessione);
    Task<string?> DocenteDellaSessioneAsync(string idSessione);
    Task<bool> IsMembroAsync(int idGruppo, string email);

    Task<Abstract?> FindAbstractByGruppoAsync(int idGruppo);
    Task AddAbstractAsync(Abstract a);
    Task UpdateAbstractAsync(Abstract a);

    Task<Progetto?> FindProgettoByGruppoAsync(int idGruppo);
    Task AddProgettoAsync(Progetto p);
    Task UpdateProgettoAsync(Progetto p);
}
