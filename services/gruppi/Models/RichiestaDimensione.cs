namespace Gruppi.Models;

public class RichiestaDimensione
{
    public int IdRichiesta { get; set; }      // PK + FK -> richieste
    public string IdSessione { get; set; } = "";
    public int IdGruppo { get; set; }
    public string Descrizione { get; set; } = "";
}
