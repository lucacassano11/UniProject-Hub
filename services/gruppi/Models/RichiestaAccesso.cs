namespace Gruppi.Models;

public class RichiestaAccesso
{
    public int IdRichiesta { get; set; }      // PK + FK -> richieste
    public int IdGruppo { get; set; }
    public string StudenteEmail { get; set; } = "";
}
