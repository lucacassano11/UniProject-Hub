namespace Log.Models;

public class Evento
{
    public int IdEvento { get; set; }
    public string Tipo { get; set; } = "";
    public string? AttoreEmail { get; set; }
    public string Dettaglio { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
