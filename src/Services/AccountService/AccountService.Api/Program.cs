using AccountService.Api.Middleware;
using AccountService.Application;
using AccountService.Infrastructure;
using AccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Applies migrations at startup: creates/updates the schema of the owned
// database (bank_accounts) on every boot. In Docker, compose declares
// depends_on postgres-db to guarantee availability.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "accountservice", status = "ok" }));

app.Run();

/// <summary>
/// Exposes the implicit Program class (top-level statements) to test hosts:
/// WebApplicationFactory&lt;Program&gt; requires a public entry type.
/// </summary>
public partial class Program { }