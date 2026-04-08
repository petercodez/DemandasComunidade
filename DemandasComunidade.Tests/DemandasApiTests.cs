using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace CommunityDemands.Tests;

public class DemandApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DemandApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDemands_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/demands");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task PostDemand_CreatesNewDemand()
    {
        // Arrange
        var newDemand = new { Title = "Buraco na via", Description = "Rua principal precisa de recapeamento." };

        // Act
        var response = await _client.PostAsJsonAsync("/demands", newDemand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}