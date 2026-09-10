namespace AccountService.Domain.ValueObjects;

/// <summary>
/// Movement kinds supported by the bank (spec use cases: Deposito / Retiro).
/// EF Core stores it as VARCHAR(50) using the enum name via HasConversion.
/// </summary>
public enum TipoMovimiento
{
    Deposito = 1,
    Retiro = 2
}

/// <summary>
/// Single source of truth for how each movement type contributes to the
/// balance. Both Cuenta (F3 validation) and Movimiento (post-balance)
/// derive their signed amount from here, so the rule cannot drift.
/// </summary>
public static class TipoMovimientoExtensions
{
    /// <summary>Contribution to the balance: Deposito +1, Retiro -1.</summary>
    public static int Signo(this TipoMovimiento tipoMovimiento)
        => tipoMovimiento == TipoMovimiento.Retiro ? -1 : 1;
}