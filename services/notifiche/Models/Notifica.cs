namespace Notifiche.Models;

public class Notifica
{
    public int IdNotifica { get; set; }
    public string DestinatarioEmail { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Messaggio { get; set; } = "";
    public bool Letta { get; set; }
    public DateTime CreatedAt { get; set; }
}
