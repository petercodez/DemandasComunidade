using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DemandasComunidade.Tests;

public class DemandApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DemandApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDemands_ReturnsSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/demands");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task PostDemand_CreatesNewDemand()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                        .ConfigurePrimaryHttpMessageHandler(() => new MockBrasilApiHandler());
            });
        }).CreateClient();

        var newDemand = new
        {
            Title = "Buraco na via",
            Description = "Rua principal precisa de recapeamento.",
            Cep = "01001000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/demands", newDemand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ---------------------------------------------------------
    // NOVO TESTE DE INTEGRAÇÃO: Valida o retorno da Brasil API
    // ---------------------------------------------------------
    [Fact]
    public async Task PostDemand_ComCepValido_DeveRetornarCreatedEEnderecoPreenchido()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                        .ConfigurePrimaryHttpMessageHandler(() => new MockBrasilApiHandler());
            });
        }).CreateClient();

        var newDemand = new
        {
            Title = "Teste de Integração Mockado",
            Description = "Garantindo que o fluxo não quebra",
            Cep = "01001000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/demands", newDemand);

        // Assert
        response.EnsureSuccessStatusCode();

        // Valida se o endereço foi montado usando o Mock
        var returnedDemand = await response.Content.ReadFromJsonAsync<Demand>();

        Assert.NotNull(returnedDemand);
        Assert.Equal("Praça da Sé, Sé - São Paulo/SP", returnedDemand.Location);
    }
}