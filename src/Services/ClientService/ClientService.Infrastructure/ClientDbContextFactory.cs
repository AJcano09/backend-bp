using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClientService.Infrastructure;

/// <summary>
/// Design-time factory: enables generating/applying migrations with
/// 'dotnet ef' without booting the API (standard pattern).
/// </summary>
public sealed class ClientDbContextFactory : IDesignTimeDbContextFactory<ClientDbContext>
{
    public ClientDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClientDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=bank_clients;Username=admin;Password=adminpassword")
            .Options;

        return new ClientDbContext(options);
    }
}