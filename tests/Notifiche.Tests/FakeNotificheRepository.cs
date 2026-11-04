using Notifiche.Data;
using Notifiche.Models;

public class FakeNotificheRepository : INotificheRepository
{
    public readonly List<Notifica> Notifiche = new();
    private int _nextId = 1;

    public Notifica Seed(string email, string tipo, string messaggio, bool letta = false)
    {
        var n = new Notifica
        {
            IdNotifica = _nextId++, DestinatarioEmail = email, Tipo = tipo,
            Messaggio = messaggio, Letta = letta, CreatedAt = DateTime.UtcNow,
        };
        Notifiche.Add(n);
        return n;
    }

    public Task<List<Notifica>> ListAsync(string email, bool soloNonLette) =>
        Task.FromResult(Notifiche
            .Where(n => n.DestinatarioEmail == email && (!soloNonLette || !n.Letta))
            .OrderByDescending(n => n.CreatedAt).ToList());

    public Task<int> CountNonLetteAsync(string email) =>
        Task.FromResult(Notifiche.Count(n => n.DestinatarioEmail == email && !n.Letta));

    public Task<Notifica?> FindAsync(int idNotifica) =>
        Task.FromResult(Notifiche.FirstOrDefault(n => n.IdNotifica == idNotifica));

    public Task MarkLettaAsync(Notifica n) { n.Letta = true; return Task.CompletedTask; }
}
