using System.Net;
using System.Net.Http.Json;

using DemandasComunidade.Api;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit;

namespace DemandasComunidade.Tests;

public class DemandApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DemandApiTests(WebApplicationFactory<Program> factory)
    {
        // Configura o servidor de testes uma única vez para todos os métodos
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // 1. Remove a conexão real com o PostgreSQL (Supabase)
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                // 2. Cria um banco de dados temporário na memória RAM para os testes
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString()));

                // 3. Configura o HttpClient para usar o Mock da Brasil API
                services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                    .ConfigurePrimaryHttpMessageHandler(() => new MockBrasilApiHandler());
            });
        });
    }

    [Fact]
    public async Task GetDemands_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
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
        var client = _factory.CreateClient();

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

    [Fact]
    public async Task PostDemand_ComCepValido_DeveRetornarCreatedEEnderecoPreenchido()
    {
        // Arrange
        var client = _factory.CreateClient();

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