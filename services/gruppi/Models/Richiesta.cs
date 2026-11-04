namespace Gruppi.Models;

public class Richiesta
{
    public int IdRichiesta { get; set; }
    public TipoRichiesta Tipo { get; set; }
    public bool? Esito { get; set; }          // null = pendente
    public DateTime CreatedAt { get; set; }
}
