using Gruppi.Models;

namespace Gruppi.Data;

public interface IGruppoRepository
{
    // letture cross-dominio (DB condiviso, sola lettura)
    Task<bool> SessioneExistsAsync(string idSessione);
    Task<bool> StudenteExistsAsync(string email);
    Task<string?> DocenteDellaSessioneAsync(string idSessione);

    // gruppi
    Task<Gruppo?> FindAsync(int idGruppo);
    Task<List<Gruppo>> ListBySessioneAsync(string idSessione);
    Task<Gruppo?> FindGruppoDiStudenteInSessioneAsync(string email, string idSessione);
    Task AddAsync(Gruppo gruppo);
    Task UpdateAsync(Gruppo gruppo);
    Task DeleteAsync(Gruppo gruppo);

    // membri
    Task<List<MembroGruppo>> ListMembriAsync(int idGruppo);
    Task<bool> IsMembroAsync(int idGruppo, string email);
    Task<int> CountMembriAsync(int idGruppo);
    Task AddMembroAsync(MembroGruppo membro);
    Task RemoveMembroAsync(MembroGruppo membro);
    Task<List<MembroGruppo>> ListGruppiDiStudenteAsync(string email);

    // richieste (base + specializzazioni)
    Task<Richiesta?> FindRichiestaAsync(int idRichiesta);
    Task UpdateRichiestaAsync(Richiesta richiesta);

    Task<int> AddInvitoAsync(Richiesta richiesta, Invito invito);   // ritorna idRichiesta
    Task<Invito?> FindInvitoAsync(int idRichiesta);
    Task<List<(Richiesta, Invito)>> ListInvitiPerStudenteAsync(string email, bool soloPendenti);

    Task<int> AddAccessoAsync(Richiesta richiesta, RichiestaAccesso accesso);
    Task<RichiestaAccesso?> FindAccessoAsync(int idRichiesta);
    Task<List<(Richiesta, RichiestaAccesso)>> ListAccessiPerGruppoAsync(int idGruppo, bool soloPendenti);

    Task<int> AddDimensioneAsync(Richiesta richiesta, RichiestaDimensione dimensione);
    Task<List<(Richiesta, RichiestaDimensione)>> ListDimensioniPerGruppoAsync(int idGruppo);

    // pulizia richieste alla cancellazione gruppo
    Task DeleteRichiesteDiGruppoAsync(int idGruppo);
}
