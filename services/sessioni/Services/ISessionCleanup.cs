namespace Sessioni.Services;

public interface ISessionCleanup
{
    Task<int> RunOnceAsync(DateOnly now);
}
