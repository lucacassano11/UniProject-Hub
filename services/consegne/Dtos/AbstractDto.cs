namespace Consegne.Dtos;

public record AbstractDto(int IdAbstract, int IdGruppo, string Descrizione,
    DateTime DataOraConsegna, string? ModificheProposte, bool? Esito);
