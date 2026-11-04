namespace UniProjectHub.Shared;

/// <summary>Registra un evento nel log centralizzato (best-effort, scrittura sul DB condiviso).</summary>
public interface IEventoLogger
{
    Task LogAsync(string tipo, string? attoreEmail, string dettaglio);
}
