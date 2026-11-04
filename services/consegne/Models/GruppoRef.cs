namespace Consegne.Models;

public class GruppoRef
{
    public int IdGruppo { get; set; }
    public string IdSessione { get; set; } = "";
    public StatoGruppo Stato { get; set; }
    public string? CapogruppoEmail { get; set; }
}
