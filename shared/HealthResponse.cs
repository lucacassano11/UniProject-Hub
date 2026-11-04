namespace UniProjectHub.Shared;

/// <summary>Risposta standard dell'endpoint /health di ogni servizio.</summary>
public record HealthResponse(string Service, string Status)
{
    public static HealthResponse Ok(string service) => new(service, "ok");
}
