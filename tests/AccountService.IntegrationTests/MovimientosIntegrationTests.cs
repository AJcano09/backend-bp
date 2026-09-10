using System.Net;
using System.Net.Http.Json;
using AccountService.Application.DTOs;
using Xunit;

namespace AccountService.IntegrationTests;

/// <summary>
/// F6: end-to-end coverage of the critical business flow (F2/F3) against a
/// real PostgreSQL container and real HTTP — no mocks. Two scenario-cases of
/// the statement: deposit updates the available balance; a withdrawal without
/// funds returns 400 with the exact F3 message.
/// </summary>
public class MovimientosIntegrationTests : IClassFixture<AccountApiFixture>
{
    private readonly AccountApiFixture _fixture;

    public MovimientosIntegrationTests(AccountApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task RegistrarDeposito_ActualizaElSaldoDisponible()
    {
        await _fixture.SeedClienteAsync(1, "Jose Lema");
        var client = _fixture.CreateClient();

        var cuenta = await client.PostAsJsonAsync("/api/cuentas", new CreateCuentaRequest(478758, "Ahorros", 2000, 1));
        cuenta.EnsureSuccessStatusCode();

        var movimiento = await client.PostAsJsonAsync("/api/movimientos",
            new RegistrarMovimientoRequest(478758, "Deposito", 600));

        Assert.Equal(HttpStatusCode.Created, movimiento.StatusCode);
        var body = await movimiento.Content.ReadFromJsonAsync<MovimientoResponse>();
        Assert.Equal(2600, body!.Saldo);

        // Persistence regression guard: the movement must survive in the
        // real database (the response alone could be an in-memory projection;
        // a repository Update that never saved would still pass the check
        // above and lose the write).
        var ledger = await client.GetFromJsonAsync<List<MovimientoResponse>>("/api/movimientos?cuenta=478758");
        var persisted = Assert.Single(ledger!);
        Assert.Equal(600, persisted.Valor);
        Assert.Equal(2600, persisted.Saldo);
        Assert.NotEqual(0, persisted.Id); // identity fix-up proves the INSERT ran
    }

    [Fact]
    public async Task RegistrarRetiroSinFondos_Devuelve400YMensajeSaldoNoDisponible()
    {
        await _fixture.SeedClienteAsync(2, "Marianela Montalvo");
        var client = _fixture.CreateClient();

        await client.PostAsJsonAsync("/api/cuentas", new CreateCuentaRequest(496825, "Ahorros", 540, 2));

        var response = await client.PostAsJsonAsync("/api/movimientos",
            new RegistrarMovimientoRequest(496825, "Retiro", 600));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var texto = await response.Content.ReadAsStringAsync();
        Assert.Contains("Saldo no disponible", texto);
    }
}