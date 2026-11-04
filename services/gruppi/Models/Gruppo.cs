namespace Gruppi.Models;

public class Gruppo
{
    public int IdGruppo { get; set; }
    public string IdSessione { get; set; } = "";
    public int Dimensione { get; set; } = 3;
    public StatoGruppo Stato { get; set; } = StatoGruppo.NUOVO;
    public string? CapogruppoEmail { get; set; }
}
