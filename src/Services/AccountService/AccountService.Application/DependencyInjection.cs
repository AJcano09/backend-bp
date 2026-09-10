using AccountService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CuentaService>();
        services.AddScoped<ReporteService>();
        return services;
    }
}