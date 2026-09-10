using ClientService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClientService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClienteService>();
        return services;
    }
}