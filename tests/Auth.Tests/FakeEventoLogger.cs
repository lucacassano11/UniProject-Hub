using UniProjectHub.Shared;

public class FakeEventoLogger : IEventoLogger
{
    public readonly List<(string Tipo, string? Attore, string Dettaglio)> Eventi = new();

    public Task LogAsync(string tipo, string? attoreEmail, string dettaglio)
    {
        Eventi.Add((tipo, attoreEmail, dettaglio));
        return Task.CompletedTask;
    }
}
