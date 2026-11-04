namespace Consegne.Models;

public class Progetto
{
    public int IdProgetto { get; set; }
    public int IdGruppo { get; set; }
    public string? DocenteEmail { get; set; }
    public string FileZip { get; set; } = "";
    public DateTime DataOraConsegna { get; set; }
    public int? Valutazione { get; set; }
    public bool? Accettazione { get; set; }
}
