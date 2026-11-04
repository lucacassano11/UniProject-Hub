namespace Consegne.Dtos;

public record ProgettoDto(int IdProgetto, int IdGruppo, string FileZip,
    DateTime DataOraConsegna, int? Valutazione, bool? Accettazione);
