namespace Consegne.Dtos;

public record ConsegnaGruppoDto(int IdGruppo, string Stato, AbstractDto? Abstract, ProgettoDto? Progetto);
