namespace AccountService.Domain.ValueObjects;

/// <summary>
/// Account types supported by the bank (spec use cases: Ahorros / Corriente).
/// EF Core stores it as VARCHAR(50) using the enum name via HasConversion.
/// </summary>
public enum TipoCuenta
{
    Ahorros = 1,
    Corriente = 2
}