namespace Gruppi.Dtos;

public record GruppoDto(
    int IdGruppo, string IdSessione, int Dimensione, string Stato,
    string? CapogruppoEmail, int NumeroMembri, List<MembroDto> Membri);
