using Microsoft.EntityFrameworkCore;

namespace DemandasComunidade.Api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Demand> Demands => Set<Demand>();
}