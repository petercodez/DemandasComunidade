using DemandasComunidade.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados (Supabase)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection")));

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------------
// GET /demands - LER DO BANCO DE DADOS
// ---------------------------------------------------------
app.MapGet("/demands", async (AppDbContext dbContext) =>
{
    // Busca todas as demandas diretamente da tabela do PostgreSQL
    var demands = await dbContext.Demands.ToListAsync();
    return Results.Ok(demands);
});

// ---------------------------------------------------------
// POST /demands - SALVAR NO BANCO DE DADOS
// ---------------------------------------------------------
app.MapPost("/demands", async (DemandInput input, HttpClient httpClient, AppDbContext dbContext) =>
{
    // Buscar os dados na Brasil API
    var response = await httpClient.GetAsync($"https://brasilapi.com.br/api/cep/v1/{input.Cep}");

    // Se o CEP não existir (retornar 404) - barragem da requisição
    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest("CEP inválido ou não encontrado.");
    }

    // Conversão do JSON que a Brasil API devolveu para o record CepResponse
    var address = await response.Content.ReadFromJsonAsync<CepResponse>();
    if (address == null) return Results.BadRequest("Erro ao processar endereço.");

    // String de endereço para salvar no banco
    string localizacaoCompleta = $"{address.Street}, {address.Neighborhood} - {address.City}/{address.State}";

    // CRIAR A DEMANDA
    var newDemand = new Demand
    {
        Title = input.Title,
        Description = input.Description,
        Cep = input.Cep,
        Location = localizacaoCompleta
    };

    // SALVAR NO BANCO DE DADOS
    dbContext.Demands.Add(newDemand);
    await dbContext.SaveChangesAsync();

    // Retorna o status 201 Created com os dados gerados
    return Results.Created($"/demands/{newDemand.Id}", newDemand);
});

app.Run();

// ---------------------------------------------------------
// MODELOS AUXILIARES
// ---------------------------------------------------------

// O que o usuário vai enviar no corpo do POST
public record DemandInput(string Title, string Description, string Cep);

// Modelo que mapeia o JSON de resposta da Brasil API
public record CepResponse(string Cep, string State, string City, string Neighborhood, string Street);

// Necessário para expor a classe Program para o projeto de testes
public partial class Program { }