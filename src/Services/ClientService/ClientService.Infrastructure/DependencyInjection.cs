using ClientService.Application.Ports;
using ClientService.Infrastructure.Messaging;
using ClientService.Infrastructure.Options;
using ClientService.Infrastructure.Persistence;
using ClientService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ClientService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<ClientDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClienteEventPublisher, ClienteEventPublisher>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton(provider =>
        {
            var rabbitOptions = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            return new ConnectionFactory
            {
                HostName = rabbitOptions.Host,
                UserName = rabbitOptions.User,
                Password = rabbitOptions.Password
            };
        });

        return services;
    }
}