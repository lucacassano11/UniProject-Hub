using UniProjectHub.Shared;

public class FakeNotificaPublisher : INotificaPublisher
{
    public readonly List<(string Dest, string Tipo, string Messaggio)> Pubblicate = new();

    public Task PublishAsync(string destinatarioEmail, string tipo, string messaggio)
    {
        Pubblicate.Add((destinatarioEmail, tipo, messaggio));
        return Task.CompletedTask;
    }
}
