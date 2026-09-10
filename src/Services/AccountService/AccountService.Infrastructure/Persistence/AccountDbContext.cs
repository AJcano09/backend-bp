using AccountService.Domain.Entities;
using AccountService.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence;

/// <summary>
/// Account microservice DbContext. OWNED: ClientesLectura, Cuentas and
/// Movimientos tables of the bank_accounts database (database-per-service).
/// </summary>
public class AccountDbContext : DbContext
{
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    public DbSet<ClientesLectura> ClientesLectura => Set<ClientesLectura>();

    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}