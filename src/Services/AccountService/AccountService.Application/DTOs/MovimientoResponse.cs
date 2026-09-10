namespace AccountService.Application.DTOs;

/// <summary>
/// Movement read model returned by the API. Saldo is the balance AFTER the
/// movement (F2: the ledger keeps the transaction history).
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
            movimiento.Valor,
            movimiento.Saldo,
            movimiento.NumeroCuenta);
}