using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.ReadModels;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AccountService.IntegrationTests;

/// <summary>
/// Integration test host: a REAL disposable PostgreSQL (Testcontainers, not
/// EF InMemory) behind a WebApplicationFactory. The full HTTP pipeline
/// (controllers, middleware, exception handler) runs against the container;
/// only the DbContext connection is pointed at the test database. RabbitMQ is
/// intentionally absent: the event consumer retries with backoff and simply
/// logs warnings while the broker is down, so the host stays healthy.
/// </summary>
public sealed class AccountApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("bank_accounts_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
        => await _postgres.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AccountDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AccountDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    /// <summary>
    /// Seeds a client in the ClientesLectura read model (what the RabbitMQ
    /// consumer would normally mirror) so F1 (FK client exists) and F2/F3
    /// flows can run without the broker.
    /// </summary>
    public async Task SeedClienteAsync(int clienteId, string nombre)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        await db.Database.MigrateAsync();
        db.ClientesLectura.Add(new ClientesLectura(clienteId, nombre, estado: true));
        await db.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
        => await _postgres.DisposeAsync();
}