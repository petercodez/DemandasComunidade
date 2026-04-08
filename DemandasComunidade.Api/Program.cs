using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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

app.MapPost("/demands", async (DemandInput input) =>
{
    var json = await File.ReadAllTextAsync(FilePath);
    var demands = JsonSerializer.Deserialize<List<Demand>>(json) ?? new List<Demand>();

    var newDemand = new Demand(Guid.NewGuid(), input.Title, input.Description, DateTime.UtcNow);
    demands.Add(newDemand);

    await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(demands, new JsonSerializerOptions { WriteIndented = true }));

    return Results.Created($"/demands/{newDemand.Id}", newDemand);
});

app.Run();

// Modelos
public record DemandInput(string Title, string Description);
public record Demand(Guid Id, string Title, string Description, DateTime CreatedAt);

// Necessário para expor a classe Program para o projeto de testes
public partial class Program { }