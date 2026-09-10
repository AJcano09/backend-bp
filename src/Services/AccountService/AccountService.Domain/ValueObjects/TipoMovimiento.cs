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