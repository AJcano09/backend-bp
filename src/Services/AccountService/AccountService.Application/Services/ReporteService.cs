using AccountService.Application.DTOs;
using AccountService.Application.Exceptions;
using AccountService.Application.Ports;
using AccountService.Domain.ValueObjects;

namespace AccountService.Application.Services;

/// <summary>
/// F4: consolidated account statement by client and date range.
/// Returns one row per movement inside the range, with account context.
/// </summary>
public sealed class ReporteService
{
    private readonly ICuentaRepository _cuentaRepository;

    public ReporteService(ICuentaRepository cuentaRepository)
        => _cuentaRepository = cuentaRepository;

    public async Task<IReadOnlyList<ReporteMovimientoResponse>> GenerarEstadoCuentaAsync(
        int clienteId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        if (desde > hasta)
            throw new ArgumentException("La fecha 'desde' no puede ser posterior a 'hasta'.");

        if (await _cuentaRepository.ClienteActivoAsync(clienteId, cancellationToken) is null)
            throw new ClienteNotFoundException(clienteId);

        var cuentas = await _cuentaRepository.GetByClienteIdConMovimientosEnRangoAsync(
            clienteId, desde, hasta, cancellationToken);

        var nombreCliente = await _cuentaRepository.GetNombreClienteAsync(clienteId, cancellationToken);

        return cuentas
            .SelectMany(c => c.Movimientos.Select(m => new ReporteMovimientoResponse(
                m.Fecha,
                nombreCliente,
                c.NumeroCuenta,
                c.TipoCuenta.ToString(),
                c.SaldoInicial,
                c.Estado,
                SignoAplicado(m),
                m.Saldo)))
            .OrderBy(r => r.Fecha)
            .ToList();
    }

    /// <summary>
    /// The report exposes the signed magnitude (F2: values can be positive or
    /// negative): deposits positive, withdrawals negative. The stored Valor is
    /// always positive; the sign is owned by the movement type.
    /// </summary>
    private static decimal SignoAplicado(Domain.Entities.Movimiento m)
        => m.TipoMovimiento.Signo() * m.Valor;
}