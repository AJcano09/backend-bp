using AccountService.Application.Ports;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Options;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AccountService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<AccountDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICuentaRepository, CuentaRepository>();

        // Async integration with ClientService: consumes the 'clientes' topic
        // exchange and keeps the ClientesLectura read model in sync.
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddHostedService<ClienteEventConsumer>();

        return services;
    }
}