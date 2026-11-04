using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UniProjectHub.Shared;
using Xunit;

public class RoutingTests
{
    [Fact]
    public async Task Gateway_Health_Returns_Ok()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("gateway", body!.Service);
    }

    [Fact]
    public async Task Gateway_Unknown_Route_Returns_NotFound()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inesistente/x");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
