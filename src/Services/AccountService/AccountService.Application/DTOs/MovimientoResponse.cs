using AccountService.Domain.ValueObjects;

namespace AccountService.Application.DTOs;

/// <summary>
/// Movement read model returned by the API. Saldo is the balance AFTER the
/// movement (F2: the ledger keeps the transaction history). Valor exposes the
/// signed magnitude (F2: values can be positive or negative): deposits
/// positive, withdrawals negative — consistent with the /reportes endpoint.
/// The stored magnitude is always positive; the sign is owned by the type.
/// </summary>
public sealed record MovimientoResponse(
    int Id,
    DateTime Fecha,
    string TipoMovimiento,
    decimal Valor,
    decimal Saldo,
    int NumeroCuenta)
{
    public static MovimientoResponse FromDomain(Domain.Entities.Movimiento movimiento) =>
        new(
            movimiento.Id,
            movimiento.Fecha,
            movimiento.TipoMovimiento.ToString(),
            movimiento.TipoMovimiento.Signo() * movimiento.Valor,
            movimiento.Saldo,
            movimiento.NumeroCuenta);
}