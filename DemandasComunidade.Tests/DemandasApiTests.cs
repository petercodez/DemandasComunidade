using System.Net;
using System.Net.Http.Json;

using DemandasComunidade.Api;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DemandasComunidade.Tests;

public class DemandasApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DemandasApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Força o ambiente para "Testing" antes da API subir
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // Garante a criação da estrutura do banco em memória
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }

                // Configura o HttpClient para utilizar o Mock da Brasil API
                services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                    .ConfigurePrimaryHttpMessageHandler(() => new MockBrasilApiHandler());
            });
        });
    }

    [Fact]
    public async Task PostDemand_ComDadosValidos_DeveSalvarNoBancoERetornarCreated()
    {
        var client = _factory.CreateClient();
        var novaDemanda = new DemandInput("Buraco na via", "Asfalto cedendo perto do poste", "01001000");

        var response = await client.PostAsJsonAsync("/demands", novaDemanda);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var demandaCriada = await response.Content.ReadFromJsonAsync<Demand>();
        Assert.NotNull(demandaCriada);
        Assert.Equal("Buraco na via", demandaCriada.Title);
        Assert.Equal("Praça da Sé, Sé - São Paulo/SP", demandaCriada.Location);
    }

    [Fact]
    public async Task GetDemands_DeveRetornarListaDeDemandasComSucesso()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/demands");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}