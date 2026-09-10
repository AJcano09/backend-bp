using AccountService.Application.DTOs;
using AccountService.Application.Exceptions;
using AccountService.Application.Helpers;
using AccountService.Application.Ports;
using AccountService.Domain.Entities;
using AccountService.Domain.Exceptions;
using AccountService.Domain.ValueObjects;

namespace AccountService.Application.Services;

/// <summary>
/// Application use cases of the account service (F1, F2, F3). Orchestrates
/// the repository and the Cuenta aggregate; the domain owns every invariant.
/// </summary>
public sealed class CuentaService
{
    private readonly ICuentaRepository _cuentaRepository;

    public CuentaService(ICuentaRepository cuentaRepository)
    {
        _cuentaRepository = cuentaRepository;
    }

    public async Task<Cuenta> CreateAsync(CreateCuentaRequest request, CancellationToken cancellationToken = default)
    {
        var tipo = EnumParser.Parse<TipoCuenta>(request.Tipo);

        if (await _cuentaRepository.GetByNumeroCuentaAsync(request.NumeroCuenta, cancellationToken) is not null)
            throw new InvalidOperationException($"The account number {request.NumeroCuenta} already exists.");

        var clienteEstado = await WaitForClienteReadModelAsync(request.ClienteId, cancellationToken);

        if (clienteEstado is null)
            throw new ArgumentException($"The client {request.ClienteId} is not registered in the account service (ClientesLectura).");

        if (clienteEstado is false)
            throw new InvalidOperationException($"The client {request.ClienteId} is deactivated and cannot own a new account.");

        var cuenta = new Cuenta(request.NumeroCuenta, tipo, request.SaldoInicial, request.ClienteId);
        await _cuentaRepository.AddAsync(cuenta, cancellationToken);
        return cuenta;
    }

    public async Task<IReadOnlyList<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _cuentaRepository.GetAllAsync(cancellationToken);

    public async Task<Cuenta> GetByNumeroCuentaAsync(int numeroCuenta, CancellationToken cancellationToken = default)
        => await _cuentaRepository.GetByNumeroCuentaAsync(numeroCuenta, cancellationToken)
            ?? throw new CuentaNotFoundException(numeroCuenta);

    public async Task<Cuenta> UpdateAsync(int numeroCuenta, UpdateCuentaRequest request, CancellationToken cancellationToken = default)
    {
        var tipo = EnumParser.Parse<TipoCuenta>(request.Tipo);
        var cuenta = await GetByNumeroCuentaAsync(numeroCuenta, cancellationToken);

        cuenta.Update(tipo);
        await _cuentaRepository.UpdateAsync(cuenta, cancellationToken);
        return cuenta;
    }

    /// <summary>
    /// F2/F3: registers a deposit or a withdrawal. The validation lives in
    /// Cuenta.RegistrarMovimiento; an insufficient balance throws
    /// <see cref="SaldoInsuficienteException"/> and nothing is persisted.
    /// </summary>
    public async Task<Cuenta> RegistrarMovimientoAsync(RegistrarMovimientoRequest request, CancellationToken cancellationToken = default)
    {
        var tipoMovimiento = EnumParser.Parse<TipoMovimiento>(request.Tipo);
        var cuenta = await GetByNumeroCuentaAsync(request.NumeroCuenta, cancellationToken);

        cuenta.RegistrarMovimiento(tipoMovimiento, request.Valor);
        await _cuentaRepository.UpdateAsync(cuenta, cancellationToken);
        return cuenta;
    }

    public async Task<IReadOnlyList<Movimiento>> GetMovimientosAsync(int numeroCuenta, CancellationToken cancellationToken = default)
    {
        var cuenta = await GetByNumeroCuentaAsync(numeroCuenta, cancellationToken);
        return cuenta.Movimientos.OrderByDescending(m => m.Fecha).ToList();
    }

    /// <summary>
    /// Eventually-consistent guard for the ClientesLectura read model: the
    /// client event (RabbitMQ) may still be in flight when a POST /cuentas
    /// arrives right after the client was created. A short bounded wait (up to
    /// 10 seconds) absorbs the broker warm-up so the API never reports a false
    /// "client not registered"; a client that is still absent afterwards is a
    /// genuine business/integration error and keeps the original message.
    /// Returns the mirrored Estado once known: null = not yet registered,
    /// false = deactivated client (business rule rejection), true = active.
    /// </summary>
    private async Task<bool?> WaitForClienteReadModelAsync(int clienteId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var estado = await _cuentaRepository.ClienteActivoAsync(clienteId, cancellationToken);
            if (estado is not null)
                return estado;

            if (attempt == 19)
                return null;

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        return null;
    }
}