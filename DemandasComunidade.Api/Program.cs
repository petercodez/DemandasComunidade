using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

const string FilePath = "demands.json";

// Garante que o arquivo JSON exista
if (!File.Exists(FilePath))
{
    File.WriteAllText(FilePath, "[]");
}

app.MapGet("/demands", async () =>
{
    var json = await File.ReadAllTextAsync(FilePath);
    var demands = JsonSerializer.Deserialize<List<Demand>>(json) ?? new List<Demand>();
    return Results.Ok(demands);
});

app.MapPost("/demands", async (DemandInput input, HttpClient httpClient) =>
{
    // Buscar os dados na Brasil API
    // Requisição GET com base no CEP enviado pelo usuário
    var response = await httpClient.GetAsync($"https://brasilapi.com.br/api/cep/v1/{input.Cep}");

    // Se o CEP não existir (retornar 404) - barragem da requisição
    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest("CEP inválido ou não encontrado.");
    }

    // Conversão do JSON que a Brasil API devolveu para o record CepResponse
    var address = await response.Content.ReadFromJsonAsync<CepResponse>();

    // String de endereço para salvar no banco
    string localizacaoCompleta = $"{address!.Street}, {address.Neighborhood} - {address.City}/{address.State}";

    // LER O BANCO DE DADOS
    var json = await File.ReadAllTextAsync(FilePath);
    var demands = JsonSerializer.Deserialize<List<Demand>>(json) ?? new List<Demand>();

    // CRIAR A DEMANDA
    var newDemand = new Demand(
        Guid.NewGuid(),
        input.Title,
        input.Description,
        localizacaoCompleta,
        DateTime.UtcNow
    );

    demands.Add(newDemand);

    // SALVAR NO BANCO DE DADOS
    await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(demands, new JsonSerializerOptions { WriteIndented = true }));

    // Retorna o status 201 Created com os dados gerados
    return Results.Created($"/demands/{newDemand.Id}", newDemand);
});

app.Run();

// Modelos
// O que o usuário vai enviar no corpo do POST
public record DemandInput(string Title, string Description, string Cep);
public record Demand(Guid Id, string Title, string Description, string Location, DateTime CreatedAt);

// Modelo que mapeia o JSON de resposta da Brasil API
public record CepResponse(string Cep, string State, string City, string Neighborhood, string Street);

// Necessário para expor a classe Program para o projeto de testes
public partial class Program { }