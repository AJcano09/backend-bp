using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Persistence;

/// <summary>
/// Client microservice DbContext. OWNED: only the Personas and Clientes
/// tables of the bank_clients database (database-per-service).
/// </summary>
public class ClientDbContext : DbContext
{
    public DbSet<Persona> Personas => Set<Persona>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public ClientDbContext(DbContextOptions<ClientDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}