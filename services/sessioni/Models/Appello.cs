namespace Sessioni.Models;

public class Appello
{
    public int IdAppello { get; set; }
    public string IdSessione { get; set; } = "";
    public DateTime DataOraAppello { get; set; }
    public DateOnly ScadenzaConsegnaProgetto { get; set; }
}
