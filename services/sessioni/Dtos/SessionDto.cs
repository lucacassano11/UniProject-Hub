namespace Sessioni.Dtos;

public record SessionDto(
    string IdSessione, int VotoMassimo, DateOnly DataCreazione,
    DateOnly? InizioInvioAbstract, int NumeroAppelli);
