namespace UniProjectHub.Shared;

/// <summary>Pubblica una notifica per un destinatario (best-effort, scrittura sul DB condiviso).</summary>
public interface INotificaPublisher
{
    Task PublishAsync(string destinatarioEmail, string tipo, string messaggio);
}
