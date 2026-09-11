using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace ClientService.Infrastructure;

/// <summary>
/// Design-time factory: enables generating/applying migrations with
/// 'dotnet ef' without booting the API (standard pattern).
/// </summary>
public sealed class ClientDbContextFactory : IDesignTimeDbContextFactory<ClientDbContext>
{
    public ClientDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionStrings__DefaultConnection' no encontrado. "
                + "Defínela en el archivo .env del servicio (src/Services/ClientService/.env) o como variable de entorno, "
                + "ejemplo: ConnectionStrings__DefaultConnection=Host=postgres-db;Port=5432;Database=bank_clients;Username=admin;Password=changeme.");

        var options = new DbContextOptionsBuilder<ClientDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ClientDbContext(options);
    }
}