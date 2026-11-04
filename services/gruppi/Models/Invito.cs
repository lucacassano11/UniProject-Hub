namespace Gruppi.Models;

public class Invito
{
    public int IdRichiesta { get; set; }      // PK + FK -> richieste
    public int IdGruppo { get; set; }
    public string StudenteEmail { get; set; } = "";
}
