using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UniProjectHub.Shared;
using Xunit;

public class HealthTests
{
    [Fact]
    public async Task Auth_Health_Returns_Ok()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("auth", body!.Service);
        Assert.Equal("ok", body.Status);
    }
}
