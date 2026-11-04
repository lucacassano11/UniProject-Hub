namespace Sessioni.Models;

public class Sessione
{
    public string IdSessione { get; set; } = "";
    public int VotoMassimo { get; set; }
    public DateOnly DataCreazioneSessione { get; set; }
    public DateOnly? InizioInvioAbstract { get; set; }
    public string DocenteEmail { get; set; } = "";
}
