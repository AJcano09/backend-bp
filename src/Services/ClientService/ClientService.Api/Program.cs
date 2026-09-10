using ClientService.Api.Middleware;
using ClientService.Application;
using ClientService.Infrastructure;
using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Maps exceptions to a consistent ProblemDetails contract (see handler).
app.UseExceptionHandler();

// Applies migrations at startup: creates/updates the schema of the owned
// database (bank_clients) on every boot. In Docker, compose declares
// depends_on postgres-db to guarantee availability.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClientDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "clientservice", status = "ok" }));

app.Run();