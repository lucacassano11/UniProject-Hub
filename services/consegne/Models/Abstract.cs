namespace Consegne.Models;

public class Abstract
{
    public int IdAbstract { get; set; }
    public int IdGruppo { get; set; }
    public string? DocenteEmail { get; set; }
    public string Descrizione { get; set; } = "";
    public DateTime DataOraConsegna { get; set; }
    public string? ModificheProposte { get; set; }
    public bool? Esito { get; set; }
}
